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

		// Inherited via IVisual
		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);
		virtual event System::Action ^ Invalidated;
	private: 
		SlabVisualPrivates* privates;
		Entities::ImageSet^ images;
	};

}