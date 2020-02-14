#include "Conversions.h"

void ConvertMatrix(Matrix^ sourceMatrix, double targetMatrix[16])
{
    for (int i = 0; i < 16; i++)
    {
        targetMatrix[i] = sourceMatrix[i];
    }
}