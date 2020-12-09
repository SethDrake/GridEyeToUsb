#include <thermal.h>
#include <cstring>

IRSensor::IRSensor(): dots{}, coldDotIndex(0), hotDotIndex(0)
{
	this->i2cHandler = NULL;
	this->minTemp = 0;
	this->maxTemp = 0;
}

IRSensor::~IRSensor()
{
}

float IRSensor::rawHLtoTemp(const uint8_t rawL, const uint8_t rawH, const float coeff)
{
	const uint16_t therm = ((rawH & 0x07) << 8) | rawL;
	float temp = therm * coeff;
	if ((rawH >> 3) != 0)
	{
		temp *= -1;
	}
	return temp;
}


void IRSensor::I2Cx_WriteData(uint8_t Addr, uint8_t Reg, uint8_t Value)
{
	HAL_I2C_Mem_Write(i2cHandler, Addr, (uint16_t)Reg, I2C_MEMADD_SIZE_8BIT, &Value, 1, 10000); 
}


uint8_t IRSensor::I2Cx_ReadData(uint8_t Addr, uint8_t Reg)
{
	uint8_t value = 0;
	HAL_I2C_Mem_Read(i2cHandler, Addr, Reg, I2C_MEMADD_SIZE_8BIT, &value, 1, 10000);
	return value;
}

void IRSensor::init(I2C_HandleTypeDef* i2cHandler)
{
	this->i2cHandler = i2cHandler;
	I2Cx_WriteData(GRID_EYE_ADDR, 0x00, 0x00); //set normal mode
	I2Cx_WriteData(GRID_EYE_ADDR, 0x02, 0x00); //set 10 FPS mode
	I2Cx_WriteData(GRID_EYE_ADDR, 0x03, 0x00);  //disable INT
}


float IRSensor::readThermistor()
{
	const uint8_t thermL = I2Cx_ReadData(GRID_EYE_ADDR, 0x0E);
	const uint8_t thermH = I2Cx_ReadData(GRID_EYE_ADDR, 0x0F);
	return this->rawHLtoTemp(thermL, thermH, THERM_COEFF);
}

void IRSensor::readImage()
{
	uint8_t taddr = 0x80;
	for (uint8_t i = 0; i < 64; i++)
	{
		const uint8_t rawL = I2Cx_ReadData(GRID_EYE_ADDR, taddr);  //low
		taddr++;
		const uint8_t rawH = I2Cx_ReadData(GRID_EYE_ADDR, taddr);  //high
		taddr++;
		this->dots[i] = this->rawHLtoTemp(rawL, rawH, TEMP_COEFF);
	}
	this->findMinAndMaxTemp();
}

float* IRSensor::getTempMap()
{
	return this->dots;
}

float IRSensor::getMaxTemp()
{
	return this->maxTemp;
}

float IRSensor::getMinTemp()
{
	return this->minTemp;
}

uint8_t IRSensor::getHotDotIndex()
{
	return this->hotDotIndex;
}

uint8_t IRSensor::getColdDotIndex()
{
	return this->coldDotIndex;
}

void IRSensor::findMinAndMaxTemp()
{
	this->minTemp = 1000;
	this->maxTemp = -100;
	for (uint8_t i = 0; i < 64; i++)
	{
		if (dots[i] < minTemp)
		{
			minTemp = dots[i];	
			coldDotIndex = i;
		}
		if (dots[i] > maxTemp)
		{
			maxTemp = dots[i];
			hotDotIndex = i;
		}
	}
}