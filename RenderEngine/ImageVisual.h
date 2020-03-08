#pragma once

#include "IVisual.h"

#include <vtkImageSliceMapper.h>
#include <vtkImageSlice.h>

class ImageVisualPrivates;

namespace RenderEngine
{
    public ref class ImageVisual : public IVisual
    {
    public:
        ImageVisual(ImageSet^ images);
        !ImageVisual();
        ~ImageVisual();

		virtual void PreRender(ViewportRenderer^ viewport);

        virtual void AddTo(ViewportRenderer ^ viewport);
        virtual void RemoveFrom(ViewportRenderer ^ viewport);

        double GetWindowLevel();
        double GetWindowWidth();
        void SetWindowing(double level, double width);

        int GetImageIndex();
        int GetNumberOfImages();
        void SetImageIndex(int index);

		virtual event System::Action ^ Invalidated;

    private:
        ImageVisualPrivates* privates;

        ImageSet^ images;
        int numberOfImages;
        int currentImageIndex;
    };
}
