#pragma once

#include "IVisual.h"

#include <vtkActor2D.h>
#include <vtkImageMapper.h>

namespace RenderEngine
{
	public ref class ImageVisual : public IVisual
	{
	public:
		ImageVisual(System::Collections::Generic::List<ImageData^>^ images);

		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);

		int GetImageIndex();
		int GetNumberOfImages();
		void SetImageIndex(int index);

	private:
		vtkActor2D* image;
		vtkImageMapper* mapper;

		int numberOfImages;
		int currentImageIndex;
	};

}
