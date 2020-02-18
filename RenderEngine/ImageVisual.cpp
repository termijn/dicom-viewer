#include "ImageVisual.h"

#include <vtkRenderWindow.h>
#include <vtkWindowToImageFilter.h>
#include <vtkImageMapper3D.h>
#include <vtkImageActor.h>
#include <vtkImageProperty.h>
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


	image = vtkImageSlice::New();
	mapper = vtkImageSliceMapper::New();

	mapper->SetInputData(reader->GetOutput());
	image->SetMapper(mapper);

	vtkImageProperty* imageProperty = vtkImageProperty::New();
	imageProperty->SetColorWindow(65000);
	imageProperty->SetColorLevel(32000);
	image->SetProperty(imageProperty);
	currentImageIndex = (images->Count - 1) / 2;
	mapper->SetSliceNumber(currentImageIndex);
}

void RenderEngine::ImageVisual::AddTo(ViewportRenderer ^ viewport)
{
	renderer = viewport->GetRenderer();
	viewport->GetRenderer()->AddActor(image);
	viewport->GetRenderer()->ResetCamera();
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
	mapper->SetSliceNumber(currentImageIndex);
}
