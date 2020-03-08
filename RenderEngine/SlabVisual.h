#pragma once

#include "ViewportRenderer.h"
#include "IVisual.h"

namespace RenderEngine
{
	class SlabVisualPrivates;

	public ref class SlabVisual: public IVisual
	{
	public:
		SlabVisual(Entities::ImageSet^ images);
		!SlabVisual();
		~SlabVisual();

		double GetWindowLevel();
		double GetWindowWidth();
		void SetWindowing(double level, double width);

		double GetSlabThickness();
		void SetSlabThickness(double thicknessInMM);

		virtual void PreRender(ViewportRenderer^ viewport);
		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);
		virtual event System::Action ^ Invalidated;
	private: 
		SlabVisualPrivates* privates;
		Entities::ImageSet^ images;		
	};

}