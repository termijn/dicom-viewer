#include "ImageVisual.h"

#include <vtkRenderWindow.h>
#include <vtkWindowToImageFilter.h>
#include <vtkImageMapper3D.h>
#include <vtkImageActor.h>
#include <vtkImageProperty.h>
#include <vtkSmartPointer.h>
#include "vtkMemoryImageReader.h"

class ImageVisualPrivates
{
public:
    vtkSmartPointer<vtkImageSliceMapper> mapper;
    vtkSmartPointer<vtkImageSlice> image;
    vtkSmartPointer<vtkImageProperty> imageProperty;
    vtkRenderer* renderer;
    int numberOfImages;
    int currentImageIndex;
};

RenderEngine::ImageVisual::ImageVisual(System::Collections::Generic::List<ImageData^>^ images)
{
    privates = new ImageVisualPrivates();

    this->images = images;
    auto reader = vtkSmartPointer<vtkMemoryImageReader>::New();

    numberOfImages = images->Count;
    int width = images[0]->Width;
    int height = images[0]->Height;

    void** pixels = new void*[images->Count];
    int i = 0;
    for each(auto image in images)
    {
        pixels[i++] = image->Pixels.ToPointer();
    }

    auto firstImage = images[0];
    reader->SetImages(pixels, width, height, images->Count, firstImage->Intercept, firstImage->Slope, firstImage->PixelSpacing->X, firstImage->PixelSpacing->Y);
    reader->Update();

    privates->image = vtkSmartPointer<vtkImageSlice>::New();
    privates->mapper = vtkSmartPointer<vtkImageSliceMapper>::New();

    privates->mapper->SetInputData(reader->GetOutput());
    privates->image->SetMapper(privates->mapper);

    privates->imageProperty = vtkSmartPointer<vtkImageProperty>::New();
    privates->imageProperty->SetInterpolationTypeToCubic();
    privates->image->SetProperty(privates->imageProperty);
    SetImageIndex((images->Count - 1) / 2);
}

RenderEngine::ImageVisual::!ImageVisual()
{
    delete privates;
}

RenderEngine::ImageVisual::~ImageVisual()
{
    this->!ImageVisual();
}

void RenderEngine::ImageVisual::AddTo(ViewportRenderer ^ viewport)
{
    privates->renderer = viewport->GetRenderer();
    viewport->GetRenderer()->AddActor(privates->image);
}

void RenderEngine::ImageVisual::RemoveFrom(ViewportRenderer ^ viewport)
{
    viewport->GetRenderer()->RemoveActor(privates->image);
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
    privates->mapper->SetSliceNumber(currentImageIndex);

    auto image = images[index];
    if (image->DefaultWindowingAvailable)
    {
        privates->imageProperty->SetColorWindow(image->WindowWidth);
        privates->imageProperty->SetColorLevel(image->WindowLevel);
    }
}
