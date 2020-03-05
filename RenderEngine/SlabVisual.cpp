#include "SlabVisual.h"

#include "vtkMemoryImageReader.h"
#include <vtkSmartPointer.h>
#include <vtkImageSlice.h>
#include <vtkImageResliceMapper.h>
#include <vtkPlane.h>
#include <vtkImageProperty.h>
#include <vtkMatrix4x4.h>
#include "ImageSetReaderFactory.h"

class RenderEngine::SlabVisualPrivates 
{
public:
	vtkMemoryImageReader* imageReader;
	vtkSmartPointer<vtkImageResliceMapper> mapper;
	vtkSmartPointer<vtkImageSlice> imageSlice;
	vtkSmartPointer<vtkImageProperty> imageProperty;
};

RenderEngine::SlabVisual::SlabVisual(Entities::ImageSet ^ images)
{
	privates = new SlabVisualPrivates();

	this->images = images;

	privates->imageReader = ImageSetReaderFactory::Acquire(images);
	
	privates->imageSlice = vtkSmartPointer<vtkImageSlice>::New();
	privates->mapper = vtkSmartPointer<vtkImageResliceMapper>::New();

	privates->mapper->SetInputData(privates->imageReader->GetOutput());
	privates->imageSlice->SetMapper(privates->mapper);

	privates->imageProperty = vtkSmartPointer<vtkImageProperty>::New();
	privates->imageProperty->SetInterpolationTypeToCubic();	
	privates->imageProperty->SetColorWindow(2000);
	privates->imageProperty->SetColorLevel(500);

	privates->imageSlice->SetProperty(privates->imageProperty);

	double center[3];
	privates->imageReader->GetOutput()->GetCenter(center);
	double spacing[3];
	privates->imageReader->GetOutput()->GetSpacing(spacing);

	vtkSmartPointer<vtkPlane> plane = vtkSmartPointer<vtkPlane>::New();
	//plane->SetOrigin(center);

	double normal[3];
	normal[0] = 0;
	normal[1] = 1;
	normal[2] = 0;
	//plane->SetNormal(normal);

	
	//privates->mapper->SetSlabThickness(2);
	privates->mapper->SetSlabTypeToMax();
	privates->mapper->SliceFacesCameraOn();
	privates->mapper->SliceAtFocalPointOn();
	privates->mapper->AutoAdjustImageQualityOff();
	privates->mapper->BackgroundOff();

	auto rotation = images->TransformationToPatient;
	auto position = images->Slices[0]->PositionPatient;
	auto transform = Matrix::Translation(position) * rotation;

	vtkSmartPointer<vtkMatrix4x4> userMatrix = vtkSmartPointer<vtkMatrix4x4>::New();
	for (int y = 0; y < 4; y++)
	{
		for (int x = 0; x < 4; x++)
		{
			userMatrix->SetElement(x, y, transform[y * 4 + x]);
		}
	}
	privates->imageSlice->SetUserMatrix(userMatrix);

}

void RenderEngine::SlabVisual::AddTo(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->AddViewProp(privates->imageSlice);
}

void RenderEngine::SlabVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->RemoveViewProp(privates->imageSlice);
}


RenderEngine::SlabVisual::!SlabVisual()
{
	delete privates;
	ImageSetReaderFactory::Release(images);
}

RenderEngine::SlabVisual::~SlabVisual()
{
	this->!SlabVisual();
}