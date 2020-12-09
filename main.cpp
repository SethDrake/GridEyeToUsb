#ifdef __cplusplus
extern "C"
{
#endif
	#include <usbd_core.h>
	#include <usbd_cdc.h>
	#include "usbd_cdc_if.h"
	#include <usbd_desc.h>
	#include <thermal.h>

#define I2C_PORT		GPIOB
#define I2C_SCL			GPIO_PIN_6
#define I2C_SDA			GPIO_PIN_7

#define LD4_Pin GPIO_PIN_12
#define LD3_Pin GPIO_PIN_13
#define LD5_Pin GPIO_PIN_14
#define LD6_Pin GPIO_PIN_15

	I2C_HandleTypeDef hi2c1;

	USBD_HandleTypeDef USBD_Device;
	IRSensor irSensor;
	void Error_Handler(void);
	void SysTick_Handler(void);
	static void GPIO_Config(void);
	static void MX_I2C1_Init(void);
	void OTG_FS_IRQHandler(void);
	void OTG_HS_IRQHandler(void);
	extern PCD_HandleTypeDef hpcd;
	
	int VCP_read(void *pBuffer, int size);
	int VCP_write(const void *pBuffer, int size);
	extern char g_VCPInitialized;
	
#ifdef __cplusplus
}
#endif

static void SystemClock_Config(void)
{
	RCC_ClkInitTypeDef RCC_ClkInitStruct;
	RCC_OscInitTypeDef RCC_OscInitStruct;

	__PWR_CLK_ENABLE();
	__HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE1);

	RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSE;
	RCC_OscInitStruct.HSEState = RCC_HSE_ON;
	RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
	RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSE;
	RCC_OscInitStruct.PLL.PLLM = 8;
	RCC_OscInitStruct.PLL.PLLN = 336;
	RCC_OscInitStruct.PLL.PLLP = RCC_PLLP_DIV2;
	RCC_OscInitStruct.PLL.PLLQ = 7;
	HAL_RCC_OscConfig(&RCC_OscInitStruct);

	RCC_ClkInitStruct.ClockType = (RCC_CLOCKTYPE_SYSCLK |
	                               RCC_CLOCKTYPE_HCLK |
	                               RCC_CLOCKTYPE_PCLK1 |
	                               RCC_CLOCKTYPE_PCLK2);

	RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
	RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
	RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV4;
	RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV2;
	HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_5);

	__HAL_RCC_GPIOA_CLK_ENABLE();
	__HAL_RCC_GPIOB_CLK_ENABLE();
	__HAL_RCC_GPIOD_CLK_ENABLE();
	__HAL_RCC_I2C1_CLK_ENABLE();
}

static void GPIO_Config(void)
{
	GPIO_InitTypeDef GPIO_InitStruct = { 0 };
	
	GPIO_InitStruct.Pin = LD4_Pin | LD3_Pin | LD5_Pin | LD6_Pin;
	GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
	GPIO_InitStruct.Pull = GPIO_NOPULL;
	GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
	HAL_GPIO_Init(GPIOD, &GPIO_InitStruct);

	GPIO_InitStruct.Pin = I2C_SCL | I2C_SDA;
	GPIO_InitStruct.Mode = GPIO_MODE_AF_OD;
	GPIO_InitStruct.Pull = GPIO_PULLUP;
	GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
	GPIO_InitStruct.Alternate = GPIO_AF4_I2C1;
	HAL_GPIO_Init(I2C_PORT, &GPIO_InitStruct);
}

static void MX_I2C1_Init(void)
{
	hi2c1.Instance = I2C1;
	hi2c1.Init.ClockSpeed = 400000;
	hi2c1.Init.DutyCycle = I2C_DUTYCYCLE_2;
	hi2c1.Init.OwnAddress1 = 0;
	hi2c1.Init.AddressingMode = I2C_ADDRESSINGMODE_7BIT;
	hi2c1.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
	hi2c1.Init.OwnAddress2 = 0;
	hi2c1.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
	hi2c1.Init.NoStretchMode = I2C_NOSTRETCH_DISABLE;
	if (HAL_I2C_Init(&hi2c1) != HAL_OK)
	{
		Error_Handler();
	}

	__HAL_RCC_I2C3_FORCE_RESET();
	__HAL_RCC_I2C3_RELEASE_RESET();
}


void SysTick_Handler(void)
{
	HAL_IncTick();
	HAL_SYSTICK_IRQHandler();
}

#ifdef USE_USB_FS
void OTG_FS_IRQHandler(void)
{
	HAL_PCD_IRQHandler(&hpcd);
}
#elif defined(USE_USB_HS)
void OTG_HS_IRQHandler(void)
{
	HAL_PCD_IRQHandler(&hpcd);
}
#else
#error USB peripheral type not defined
#endif

/*
	This is a basic USB CDC device sample based on the ST USB library.
	To test it out:
		1. Install the ST VCP drivers (http://www.st.com/web/en/catalog/tools/PF257938)
		2. Connect to the virtual COM port using SmarTTY or any other terminal program
		3. Type some characters and observe the output
	Read more about the sample: http://visualgdb.com/tutorials/arm/stm32/usb/
*/

int main(void)
{
	HAL_Init();
	SystemClock_Config();
	GPIO_Config();
	MX_I2C1_Init();
	USBD_Init(&USBD_Device, &VCP_Desc, 0);

	USBD_RegisterClass(&USBD_Device, &USBD_CDC);
	USBD_CDC_RegisterInterface(&USBD_Device, &USBD_CDC_GridEyeToUsb_fops);
	USBD_Start(&USBD_Device);

	irSensor.init(&hi2c1);

	char readed;
	float* tempMap;
	char str[6];
	for (;;)
	{
		if (VCP_read(&readed, 1) != 1) {
			continue;
		}
		if (readed != 'r') {
			continue;
		}
		irSensor.readImage();
		tempMap = irSensor.getTempMap();
		for (int i = 0; i < 64; i++) {
			sprintf(str, "%03d.%02d", (uint16_t)tempMap[i], (uint16_t)((tempMap[i] - (uint16_t)tempMap[i]) * 100));
			VCP_write(&str, 6);
		}
		VCP_write("\r\n", 2);
	}
}

void Error_Handler(void)
{
	/* USER CODE BEGIN Error_Handler_Debug */
	/* User can add his own implementation to report the HAL error return state */
	__disable_irq();
	while (1)
	{
	}
	/* USER CODE END Error_Handler_Debug */
}