#pragma once

#include "IVisual.h"

namespace RenderEngine {
	
	class SlabAxesVisualPrivates;

	public ref class SlabAxesVisual : public IVisual
	{
	public:
		SlabAxesVisual(Entities::Space^ space);
		!SlabAxesVisual();
		~SlabAxesVisual();

		virtual void PreRender(ViewportRenderer^ viewport);
		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);
		virtual event System::Action ^ Invalidated;
	private:
		SlabAxesVisualPrivates* privates;	
		Entities::Space^ space;
	};
}
