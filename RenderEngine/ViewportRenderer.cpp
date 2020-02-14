#include <Windows.h>
#include "ViewportRenderer.h"
#include <algorithm>
#include "Conversions.h"

#include <vtkAutoInit.h>
VTK_MODULE_INIT(vtkRenderingOpenGL2)
VTK_MODULE_INIT(vtkRenderingVolumeOpenGL2)

#include <vtkWin32OpenGLRenderWindow.h>
#include <vtkWindowToImageFilter.h>
#include <vtkSphereSource.h>
#include <vtkWin32OutputWindow.h>
#include <vtkCamera.h>

#define VTI_FILETYPE 1
#define MHA_FILETYPE 2

using namespace RenderEngine;
using namespace System::Data::Linq;

class RenderEngine::ViewportRendererImpl 
{
public: 
	ViewportRendererImpl() 
	{
		/*vtkSmartPointer<vtkWin32OutputWindow> outputWindow = vtkSmartPointer<vtkWin32OutputWindow>::New();
		outputWindow->SetSendToStdErr(true);
		vtkOutputWindow::SetInstance(outputWindow);*/

		renderer = vtkSmartPointer<vtkOpenGLRenderer>::New();
		renderWindow = vtkSmartPointer<vtkRenderWindow>::New();
		renderWindow->AddRenderer(this->renderer);
		renderWindow->DebugOn();

		renderWindow->SetOffScreenRendering(true);

		renderWindow->SetSize(400, 400);
		//renderWindow->SetMultiSamples(8);

		windowToImageFilter = vtkSmartPointer<vtkWindowToImageFilter>::New();
		windowToImageFilter->SetInputBufferTypeToRGBA();
		windowToImageFilter->SetInput(this->renderWindow);

		vtkCamera* camera = renderer->GetActiveCamera();
		camera->ParallelProjectionOn();
		camera->SetParallelScale(100);
	}

	~ViewportRendererImpl()
	{

	}

	void SetSize(int width, int height)
	{
		renderWindow->SetSize(width, height);
	}

	void SetCameraTransformation(double up[3], double position[3], double focalPoint[3])
	{
		vtkCamera* camera = renderer->GetActiveCamera();		
		camera->SetViewUp(up[0], up[1], up[2]);
		camera->SetPosition(position[0], position[1], position[2]);
		camera->SetFocalPoint(focalPoint[0], focalPoint[1], focalPoint[2]);
	}

	void SetZoom(double factor)
	{
		vtkCamera *camera = renderer->GetActiveCamera();
		camera->SetParallelScale(factor);
	}

	void Render(byte* pixelData)
	{
		renderWindow->Render();

		windowToImageFilter->Modified();
		windowToImageFilter->Update();

		vtkImageData* imageData = windowToImageFilter->GetOutput();
		int width = imageData->GetDimensions()[0];
		int height = imageData->GetDimensions()[1];

		unsigned char *colorsPtr =
			reinterpret_cast<unsigned char *>(imageData->GetScalarPointer());

		byte * currentRgbPtr = pixelData;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				*currentRgbPtr++ = colorsPtr[2];
				*currentRgbPtr++ = colorsPtr[1];
				*currentRgbPtr++ = colorsPtr[0];
				*currentRgbPtr++ = colorsPtr[3];
				colorsPtr += 4;
			}
		}
	}

	vtkRenderer* GetRenderer() 
	{
		return renderer.GetPointer();
	}

private:
	vtkSmartPointer<vtkOpenGLRenderer> renderer;
	vtkSmartPointer<vtkRenderWindow> renderWindow;
	vtkSmartPointer<vtkWindowToImageFilter> windowToImageFilter;
};

RenderEngine::ViewportRenderer::ViewportRenderer()
{
	impl = new ViewportRendererImpl();
}

RenderEngine::ViewportRenderer::~ViewportRenderer()
{
	this->!ViewportRenderer();
}

RenderEngine::ViewportRenderer::!ViewportRenderer()
{
	if (impl != nullptr) 
	{
		delete impl;
		impl = nullptr;
	}	
}

void RenderEngine::ViewportRenderer::Render(IntPtr pixelData)
{
	impl->Render((byte*)pixelData.ToPointer());
}

void RenderEngine::ViewportRenderer::SetSize(int width, int height)
{
    impl->SetSize(width, height);
}

void RenderEngine::ViewportRenderer::SetZoom(double factor)
{
	impl->SetZoom(factor);
}

void RenderEngine::ViewportRenderer::SetCameraTransformation(Matrix ^ transformation)
{
	Vector3^ position = transformation * gcnew Vector3(0, 0, 0);
	Vector3^ direction = transformation * gcnew Vector3(0, 0, -400);
	Vector3^ up = (transformation * gcnew Vector3(0, 1, 0)) + -position;
	
	double upArr[3] = { up->X, up->Y, up->Z };
	double positionArr[3] = { direction->X, direction->Y, direction->Z };
	double focalPointArr[3] { position->X, position->Y, position->Z };
	impl->SetCameraTransformation(upArr, positionArr, focalPointArr);
}

vtkRenderer * RenderEngine::ViewportRenderer::GetRenderer()
{
	return impl->GetRenderer();
}
