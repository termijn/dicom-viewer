#include "VolumeVisual.h"

#include <vtkOpenGLGPUVolumeRayCastMapper.h>
#include <vtkColorTransferFunction.h>
#include <vtkPiecewiseFunction.h>
#include <vtkMatrix4x4.h>
#include <vtkVolumeProperty.h>
#include "vtkDicomVolumeImageReader.h"

using namespace RenderEngine;

struct RenderEngine::VolumeVisualPrivates 
{
	void ** volumeSlices;
	int nrSlices;

	double windowWidth = 360;
	double windowLevel = 310;

	vtkSmartPointer<vtkAlgorithm> reader;
	vtkSmartPointer<vtkImageData> input;
	vtkSmartPointer<vtkVolume> volume;
	vtkSmartPointer<vtkOpenGLGPUVolumeRayCastMapper> mapper;
	vtkSmartPointer<vtkColorTransferFunction> volumeColor;
	vtkSmartPointer<vtkPiecewiseFunction> volumeScalarOpacity;
	vtkSmartPointer<vtkVolumeProperty> volumeProperty;
};


VolumeVisual::VolumeVisual(VolumeData ^ volumeData)
	: privates(new VolumeVisualPrivates())
{
	auto ptrs = volumeData->GetSlicePointers();

	privates->nrSlices = volumeData->Dimensions->Y;
	privates->volumeSlices = new void*[privates->nrSlices];
	int i = 0;
	for each(auto slice in volumeData->GetSlicePointers())
	{
		privates->volumeSlices[i++] = slice.ToPointer();
	}

	int dimensions[3];
	dimensions[0] = volumeData->Dimensions->X;
	dimensions[1] = volumeData->Dimensions->Y;
	dimensions[2] = volumeData->Dimensions->Z;

	double spacing[3];
	spacing[0] = volumeData->VoxelSpacing->X;
	spacing[1] = volumeData->VoxelSpacing->Y;
	spacing[2] = volumeData->VoxelSpacing->Z;

	privates->reader =
		vtkSmartPointer<vtkAlgorithm>::New();
	privates->input =
		vtkSmartPointer<vtkImageData>::New();

	vtkSmartPointer<vtkVolumeMemoryReader> volumeReader = vtkSmartPointer<vtkVolumeMemoryReader>::New();
	volumeReader->SetSlices(
		privates->volumeSlices,   
		volumeData->Dimensions->X, 
		volumeData->Dimensions->Y, 
		volumeData->Dimensions->Z,
		volumeData->VoxelSpacing->X,
		volumeData->VoxelSpacing->Y,
		volumeData->VoxelSpacing->Z,		
		volumeData->IsSigned, 
		volumeData->RescaleIntercept,
		volumeData->RescaleSlope);
	volumeReader->Update();
	privates->input = volumeReader->GetOutput();
	privates->reader = volumeReader;

	privates->volume = vtkSmartPointer<vtkVolume>::New();
	privates->mapper = vtkSmartPointer<vtkOpenGLGPUVolumeRayCastMapper>::New();

	privates->mapper->SetInputConnection(privates->reader->GetOutputPort());
	privates->mapper->AutoAdjustSampleDistancesOff();
	privates->mapper->SetSampleDistance(0.5);

	privates->volumeColor =
		vtkSmartPointer<vtkColorTransferFunction>::New();
	/*volumeColor->AddRGBPoint(0, 0.0, 0.0, 0.0);
	volumeColor->AddRGBPoint(500, 1.0, 0.5, 0.3);
	volumeColor->AddRGBPoint(1000, 1.0, 1.0, 1);
	volumeColor->AddRGBPoint(1150, 1.0, 1.0, 0.9);*/

	privates->volumeColor->AddRGBPoint(-2024, 0, 0, 0, 0.5, 0.0);
	privates->volumeColor->AddRGBPoint(-16, 0.73, 0.25, 0.30, 0.49, .61);
	privates->volumeColor->AddRGBPoint(641, .90, .82, .56, .5, 0.0);
	privates->volumeColor->AddRGBPoint(3071, 1, 1, 1, .5, 0.0);

	// The opacity transfer function is used to control the opacity
	// of different tissue types.
	privates->volumeScalarOpacity = vtkSmartPointer<vtkPiecewiseFunction>::New();
	privates->volumeScalarOpacity->AddPoint(0, 0.00);
	privates->volumeScalarOpacity->AddPoint(100, 0.0);
	privates->volumeScalarOpacity->AddPoint(1000, 1);
	//volumeScalarOpacity->AddPoint(1150, 0.85);

	//volumeScalarOpacity->AddPoint(-3024, 0, 0.5, 0.0);
	//volumeScalarOpacity->AddPoint(-16, 0, .49, .61);
	//volumeScalarOpacity->AddPoint(641, .72, .5, 0.0);
	//volumeScalarOpacity->AddPoint(3071, .71, 0.5, 0.0);

	// The gradient opacity function is used to decrease the opacity
	// in the "flat" regions of the volume while maintaining the opacity
	// at the boundaries between tissue types.  The gradient is measured
	// as the amount by which the intensity changes over unit distance.
	// For most medical data, the unit distance is 1mm.
	vtkSmartPointer<vtkPiecewiseFunction> volumeGradientOpacity =
		vtkSmartPointer<vtkPiecewiseFunction>::New();
	volumeGradientOpacity->AddPoint(0, 0.0);
	volumeGradientOpacity->AddPoint(50, 0.5);
	volumeGradientOpacity->AddPoint(100, 1.0);

	privates->volumeProperty = vtkSmartPointer<vtkVolumeProperty>::New();

	privates->volumeProperty->SetColor(privates->volumeColor);
	
	privates->volumeProperty->SetScalarOpacity(privates->volumeScalarOpacity);
	privates->volumeProperty->SetGradientOpacity(volumeGradientOpacity);
	privates->volumeProperty->SetInterpolationTypeToLinear();
	privates->volumeProperty->ShadeOn();
	privates->volumeProperty->SetAmbient(0.1);
	privates->volumeProperty->SetDiffuse(0.8);
	privates->volumeProperty->SetSpecular(0.2);
	privates->volumeProperty->GetSpecularPower(50);
	privates->volumeProperty->SetScalarOpacityUnitDistance(0.8919);

	privates->volume->SetProperty(privates->volumeProperty);
	privates->volume->SetMapper(privates->mapper);

	auto rotation = volumeData->TransformationToPatient;
	auto position = volumeData->Slices[0]->PositionPatient;
	auto transform = Matrix::Translation(position) * rotation;

	vtkSmartPointer<vtkMatrix4x4> userMatrix = vtkSmartPointer<vtkMatrix4x4>::New();
	for (int y = 0; y < 4; y++)
	{
		for (int x = 0; x < 4; x++)
		{
			userMatrix->SetElement(x, y, transform[y * 4 + x]);
		}
	}
	privates->volume->SetUserMatrix(userMatrix);
}

VolumeVisual::~VolumeVisual()
{
	this->!VolumeVisual();	
}

RenderEngine::VolumeVisual::!VolumeVisual()
{
	delete privates;
}

void VolumeVisual::AddTo(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->AddVolume(privates->volume);
}

void RenderEngine::VolumeVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->RemoveVolume(privates->volume);
}

void VolumeVisual::SetWindowing(double windowLevel, double windowWidth)
{
	privates->windowLevel = windowLevel;
	privates->windowWidth = windowWidth;

	privates->volumeScalarOpacity->RemoveAllPoints();

	double min = windowLevel - windowWidth / 2;
	double max = windowLevel + windowWidth / 2;
	privates->volumeScalarOpacity->AddPoint(0, 0.00, 0.5, 0);
	privates->volumeScalarOpacity->AddPoint(min, 0.0,0.5, 0.5);
	privates->volumeScalarOpacity->AddPoint(max, 1, 0.5, 0);
}

double RenderEngine::VolumeVisual::GetWindowLevel()
{
	return privates->windowLevel;
}

double RenderEngine::VolumeVisual::GetWindowWidth()
{
	return privates->windowWidth;
}
