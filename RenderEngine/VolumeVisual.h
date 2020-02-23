#pragma once

#include "ViewportRenderer.h"
#include "IVisual.h"

namespace RenderEngine 
{
	using namespace Entities;

	struct VolumeVisualPrivates;

	public ref class VolumeVisual: public IVisual
	{
	public:
		VolumeVisual(ImageSet^ volumeData);
		~VolumeVisual();
		!VolumeVisual();

		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);

		array<long>^ GetHistogram();
		void SetWindowing(double windowLevel, double windowWidth);
		double GetWindowLevel();
		double GetWindowWidth();
		virtual event System::Action ^ Invalidated;
	private:
		VolumeVisualPrivates* privates;
	};

}