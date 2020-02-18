#pragma once

#include "IVisual.h"

#include <vtkImageSliceMapper.h>
#include <vtkImageSlice.h>

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
		vtkImageSliceMapper* mapper;
		vtkImageSlice* image;
		vtkRenderer* renderer;

		int numberOfImages;
		int currentImageIndex;
	};

}
