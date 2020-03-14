#include "SlabAxesVisual.h"
#include <vtkSmartPointer.h>
#include <vtkImageSlice.h>
#include <vtkImageResliceMapper.h>
#include <vtkPlane.h>
#include <vtkCamera.h>
#include <vtkTransform.h>
#include <vtkProperty.h>
#include <vtkMatrix4x4.h>
#include <vtkPolyData.h>
#include <vtkCellData.h>
#include <vtkPolyLine.h>
#include <vtkPropPicker.h>

using namespace RenderEngine;

class RenderEngine::SlabAxesVisualPrivates
{
public:
	vtkSmartPointer<vtkPolyData> axesLinesPolyData;
	vtkSmartPointer<vtkPoints> axesPoints;
	vtkSmartPointer<vtkPolyLine> xAxisLine;
	vtkSmartPointer<vtkPolyLine> yAxisLine;
	vtkSmartPointer<vtkPolyLine> zAxisLine;
	vtkSmartPointer<vtkCellArray> lines;
	vtkRenderer* renderer;

	vtkSmartPointer<vtkPolyDataMapper> axesLinesMapper;
	vtkSmartPointer<vtkActor> axesLinesActor;
};

vtkSmartPointer<vtkPolyLine> createBox(const int offset)
{
	auto polyLine = vtkSmartPointer<vtkPolyLine>::New();
	polyLine->GetPointIds()->SetNumberOfIds(8);
	for (int i = 0; i < 4; i++)
	{
		polyLine->GetPointIds()->SetId(i * 2, offset + i);
		polyLine->GetPointIds()->SetId(i * 2 + 1, offset + (i + 1) % 4);
	}
	return polyLine;
}

SlabAxesVisual::SlabAxesVisual(Entities::Space^ space, Plane hiddenPlane)
{
	this->privates = new SlabAxesVisualPrivates();
	this->space = space;
	this->hiddenPlane = hiddenPlane;
	this->isHighlighted = false;

	privates->axesLinesPolyData = vtkSmartPointer<vtkPolyData>::New();

	privates->axesPoints = vtkSmartPointer<vtkPoints>::New();

	const double halfSize = 250;
	// X-Y plane
	privates->axesPoints->InsertNextPoint(-halfSize, halfSize, 0);
	privates->axesPoints->InsertNextPoint(halfSize, halfSize, 0);
	privates->axesPoints->InsertNextPoint(halfSize, -halfSize, 0);
	privates->axesPoints->InsertNextPoint(-halfSize, -halfSize, 0);

	// Y-Z plane
	privates->axesPoints->InsertNextPoint(0, -halfSize, halfSize);
	privates->axesPoints->InsertNextPoint(0, halfSize, halfSize);
	privates->axesPoints->InsertNextPoint(0, halfSize, -halfSize);
	privates->axesPoints->InsertNextPoint(0, -halfSize, -halfSize);

	// X-Z plane
	privates->axesPoints->InsertNextPoint(-halfSize, 0, halfSize);
	privates->axesPoints->InsertNextPoint(halfSize, 0, halfSize);
	privates->axesPoints->InsertNextPoint(halfSize, 0, -halfSize);
	privates->axesPoints->InsertNextPoint(-halfSize, 0, -halfSize);

	privates->axesLinesPolyData->SetPoints(privates->axesPoints);

	privates->xAxisLine = createBox(0);
	privates->yAxisLine = createBox(4);
	privates->zAxisLine = createBox(8);

	unsigned char red[3] = { 211, 54, 130 };
	unsigned char green[3] = { 133, 153, 0 };
	unsigned char blue[3] = { 88, 110, 117 };

	vtkSmartPointer<vtkUnsignedCharArray> colors =
		vtkSmartPointer<vtkUnsignedCharArray>::New();
	colors->SetNumberOfComponents(3);

	privates->lines = vtkSmartPointer<vtkCellArray>::New();
	if (hiddenPlane != Plane::XY)
	{
		colors->InsertNextTypedTuple(red);
		privates->lines->InsertNextCell(privates->xAxisLine);
	}

	if (hiddenPlane != Plane::YZ)
	{
		colors->InsertNextTypedTuple(green);
		privates->lines->InsertNextCell(privates->yAxisLine);
	}

	if (hiddenPlane != Plane::XZ)
	{
		colors->InsertNextTypedTuple(blue);
		privates->lines->InsertNextCell(privates->zAxisLine);
	}

	privates->axesLinesPolyData->SetLines(privates->lines);
	privates->axesLinesPolyData->GetCellData()->SetScalars(colors);

	privates->axesLinesMapper = vtkSmartPointer<vtkPolyDataMapper>::New();
	privates->axesLinesMapper->SetInputData(privates->axesLinesPolyData);

	privates->axesLinesActor = vtkSmartPointer<vtkActor>::New();
	privates->axesLinesActor->SetMapper(privates->axesLinesMapper);
	privates->axesLinesActor->GetProperty()->SetLineWidth(1.5);

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

bool RenderEngine::SlabAxesVisual::Pick(double x, double y)
{
	auto picker = vtkSmartPointer<vtkPropPicker>::New();
	picker->Pick(x, y, 0, privates->renderer);

	bool isHit = picker->GetActor() == privates->axesLinesActor;
	return isHit;
}

void RenderEngine::SlabAxesVisual::Highlight()
{
	if (isHighlighted) { return; }
	isHighlighted = true;
	privates->axesLinesActor->GetProperty()->SetLineWidth(4);
	Invalidated();
}

void RenderEngine::SlabAxesVisual::DeHighlight()
{
	if (!isHighlighted) { return; }
	isHighlighted = false;
	privates->axesLinesActor->GetProperty()->SetLineWidth(2);
	Invalidated();
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
	privates->renderer = viewport->GetRenderer();
	privates->renderer->AddActor(privates->axesLinesActor);
}

void SlabAxesVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
	privates->renderer = viewport->GetRenderer();
	privates->renderer->RemoveActor(privates->axesLinesActor);
}
