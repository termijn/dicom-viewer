#include "ImageVisual.h"

#include <vtkRenderWindow.h>
#include <vtkWindowToImageFilter.h>
#include <vtkImageMapper.h>
#include <vtkActor2D.h>
#include "vtkMemoryImageReader.h"

RenderEngine::ImageVisual::ImageVisual(System::Collections::Generic::List<ImageData^>^ images)
{
	vtkMemoryImageReader* reader = vtkMemoryImageReader::New();

	numberOfImages = images->Count;
	int width = images[0]->Width;
	int height = images[0]->Height;

	void** pixels = new void*[images->Count];
	int i = 0;
	for each(auto image in images)
	{
		pixels[i++] = image->Pixels.ToPointer();
	}

	reader->SetImages(pixels, width, height, images->Count);
	reader->Update();

	mapper = vtkImageMapper::New();
	mapper->SetInputData(reader->GetOutput());
	mapper->SetColorWindow(1000);
	mapper->SetColorLevel(500);
	currentImageIndex = (images->Count - 1) / 2;
	mapper->SetZSlice(currentImageIndex);
	image = vtkActor2D::New();
	image->SetMapper(mapper);
}

void RenderEngine::ImageVisual::AddTo(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->AddActor2D(image);
}

void RenderEngine::ImageVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
	viewport->GetRenderer()->RemoveActor2D(image);
}

int RenderEngine::ImageVisual::GetImageIndex()
{
	return currentImageIndex;
}

int RenderEngine::ImageVisual::GetNumberOfImages()
{
	return numberOfImages;
}

void RenderEngine::ImageVisual::SetImageIndex(int index)
{
	currentImageIndex = index;
	mapper->SetZSlice(index);
}
