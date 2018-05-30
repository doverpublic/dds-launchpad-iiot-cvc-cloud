$endpoint = "https://api.powerbi.com/beta/3d2d2b6f-061a-48b6-b4b3-9312d687e3a1/datasets/ac227ec0-5bfe-4184-85b1-a9643778f1e4/rows?key=zrg4K1om2l4mj97GF6T3p0ze3SlyynHWYRQMdUUSC0BWetzC7bF3RZgPMG4ukznAhGub5aPsDXuQMq540X8hZA%3D%3D"
$payload = @{
"TimestampGroup" ="2018-04-30T09:37:28.749Z"
"Timestamp" ="2018-04-30T09:37:28.749Z"
"DeviceId" ="AAAAA555555"
"BatteryLevel" =98.6
"BatteryVoltage" =98.6
"BatteryMax" =98.6
"BatteryMin" =98.6
"BatteryTarget" =98.6
"BatteryPercentage" =98.6
"BatteryPercentageMax" =98.6
"BatteryPercentageMin" =98.6
"BatteryPercentageTarget" =98.6
"Temperature" =98.6
"TemperatureMax" =98.6
"TemperatureMin" =98.6
"TemperatureTarget" =98.6
"DataPointsCount" =98.6
"MeasurementType" ="AAAAA555555"
"SensorIndex" =98.6
"Frequency" =98.6
"Magnitude" =98.6
}
Invoke-RestMethod -Method Post -Uri "$endpoint" -Body (ConvertTo-Json @($payload))