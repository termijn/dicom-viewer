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
		VolumeVisual(VolumeData^ volumeData);
		~VolumeVisual();
		!VolumeVisual();

		virtual void AddTo(ViewportRenderer ^ viewport);
		virtual void RemoveFrom(ViewportRenderer ^ viewport);

		void SetWindowing(double windowLevel, double windowWidth);
		double GetWindowLevel();
		double GetWindowWidth();
	private:
		VolumeVisualPrivates* privates;
	};

}