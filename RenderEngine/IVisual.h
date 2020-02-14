#pragma once

#include "ViewportRenderer.h"

namespace RenderEngine {

	public interface class IVisual
	{
		virtual void AddTo(ViewportRenderer^ viewport);
		virtual void RemoveFrom(ViewportRenderer^ viewport);
	};
}