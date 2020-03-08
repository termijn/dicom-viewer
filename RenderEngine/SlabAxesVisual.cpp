#include "SlabAxesVisual.h"
#include <vtkSmartPointer.h>
#include <vtkImageSlice.h>
#include <vtkImageResliceMapper.h>
#include <vtkPlane.h>
#include <vtkCamera.h>
#include <vtkTransform.h>
#include <vtkImageProperty.h>
#include <vtkMatrix4x4.h>
#include <vtkPolyData.h>
#include <vtkCellData.h>
#include <vtkLine.h>

using namespace RenderEngine;

class RenderEngine::SlabAxesVisualPrivates 
{
public:
	vtkSmartPointer<vtkPolyData> axesLinesPolyData;
	vtkSmartPointer<vtkPoints> axesPoints;
	vtkSmartPointer<vtkLine> xAxisLine;
	vtkSmartPointer<vtkLine> yAxisLine;
	vtkSmartPointer<vtkLine> zAxisLine;
	vtkSmartPointer<vtkCellArray> lines;

	vtkSmartPointer<vtkPolyDataMapper> axesLinesMapper;
	vtkSmartPointer<vtkActor> axesLinesActor;
};


SlabAxesVisual::SlabAxesVisual(Entities::Space^ space)
{
	this->privates = new SlabAxesVisualPrivates();
	this->space = space;

	privates->axesLinesPolyData = vtkSmartPointer<vtkPolyData>::New();

	privates->axesPoints = vtkSmartPointer<vtkPoints>::New();
	privates->axesPoints->InsertNextPoint(-100, 0, 0);
	privates->axesPoints->InsertNextPoint(100, 0, 0);

	privates->axesPoints->InsertNextPoint(0, -100, 0);
	privates->axesPoints->InsertNextPoint(0, 100, 0);

	privates->axesPoints->InsertNextPoint(0, 0, -100);
	privates->axesPoints->InsertNextPoint(0, 0, 100);

	privates->axesLinesPolyData->SetPoints(privates->axesPoints);

	privates->xAxisLine = vtkSmartPointer<vtkLine>::New();
	privates->xAxisLine->GetPointIds()->SetId(0, 0);
	privates->xAxisLine->GetPointIds()->SetId(1, 1);

	privates->yAxisLine = vtkSmartPointer<vtkLine>::New();
	privates->yAxisLine->GetPointIds()->SetId(0, 2);
	privates->yAxisLine->GetPointIds()->SetId(1, 3);

	privates->zAxisLine = vtkSmartPointer<vtkLine>::New();
	privates->zAxisLine->GetPointIds()->SetId(0, 4);
	privates->zAxisLine->GetPointIds()->SetId(1, 5);

	privates->lines = vtkSmartPointer<vtkCellArray>::New();
	privates->lines->InsertNextCell(privates->xAxisLine);
	privates->lines->InsertNextCell(privates->yAxisLine);
	privates->lines->InsertNextCell(privates->zAxisLine);

	privates->axesLinesPolyData->SetLines(privates->lines);

	unsigned char red[3] = { 200, 0, 0 };
	unsigned char green[3] = { 0, 200, 0 };
	unsigned char blue[3] = { 0, 0, 200 };

	vtkSmartPointer<vtkUnsignedCharArray> colors =
		vtkSmartPointer<vtkUnsignedCharArray>::New();
	colors->SetNumberOfComponents(3);
	colors->InsertNextTypedTuple(red);
	colors->InsertNextTypedTuple(green);
	colors->InsertNextTypedTuple(blue);

	privates->axesLinesPolyData->GetCellData()->SetScalars(colors);

	privates->axesLinesMapper = vtkSmartPointer<vtkPolyDataMapper>::New();
	privates->axesLinesMapper->SetInputData(privates->axesLinesPolyData);

	privates->axesLinesActor = vtkSmartPointer<vtkActor>::New();
	privates->axesLinesActor->SetMapper(privates->axesLinesMapper);

	vtkMapper::SetResolveCoincidentTopologyToPolygonOffset();
	vtkMapper::SetResolveCoincidentTopologyPolygonOffsetParameters(0.1, 1);
}

SlabAxesVisual::!SlabAxesVisual()
{
	delete privates;
}

SlabAxesVisual::~SlabAxesVisual()
{
	this->!SlabAxesVisual();
}

void SlabAxesVisual::PreRender(ViewportRenderer ^ viewport)
{
	auto matrix = space->GetTransformationToRoot();

	auto camera = viewport->GetRenderer()->GetActiveCamera();
	double* direction = camera->GetDirectionOfProjection();
	auto v = gcnew Vector3(direction[0], direction[1], direction[2]) * -10;

	auto offsettedTransform = Matrix::Translation(v) * matrix;

	auto userMatrix = vtkSmartPointer<vtkMatrix4x4>::New();
	for (int y = 0; y < 4; y++)
	{
		for (int x = 0; x < 4; x++)
		{
			userMatrix->SetElement(x, y, offsettedTransform[y * 4 + x]);
		}
	}
	privates->axesLinesActor->SetUserMatrix(userMatrix.Get());
}

void SlabAxesVisual::AddTo(ViewportRenderer ^ viewport)
{
	auto renderer = viewport->GetRenderer();
	renderer->AddActor(privates->axesLinesActor);
}

void SlabAxesVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
	auto renderer = viewport->GetRenderer();
	renderer->RemoveActor(privates->axesLinesActor);
}
