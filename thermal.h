#pragma once

#ifndef __THERMAL_H
#define __THERMAL_H

#include <stdint.h>
#include <stm32f4xx.h>

#define GRID_EYE_ADDR 0xD0
#define THERM_COEFF 0.0625
#define TEMP_COEFF 0.25

class IRSensor {
public:
	IRSensor();
	~IRSensor();
	void init(I2C_HandleTypeDef* i2cHandler);
	float readThermistor();
	void readImage();
	float* getTempMap();
	float getMaxTemp();
	float getMinTemp();
	uint8_t getHotDotIndex();
	uint8_t getColdDotIndex();
protected:
	void findMinAndMaxTemp();
	void I2Cx_WriteData(uint8_t Addr, uint8_t Reg, uint8_t Value);
	uint8_t I2Cx_ReadData(uint8_t Addr, uint8_t Reg);
private:
	I2C_HandleTypeDef* i2cHandler;
	float dots[64];
	uint8_t coldDotIndex;
	uint8_t hotDotIndex;
	float minTemp;
	float maxTemp;
	float rawHLtoTemp(uint8_t rawL, uint8_t rawH, float coeff);
};

#endif /* __THERMAL_H */


