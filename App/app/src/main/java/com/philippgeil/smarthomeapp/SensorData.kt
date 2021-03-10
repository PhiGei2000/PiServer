package com.philippgeil.smarthomeapp

import java.time.LocalDateTime

public class TemperatureData {
    public lateinit var time : LocalDateTime
    public var temperature  = 0.0f

    constructor(time : LocalDateTime, temperature : Float) {
        this.time = time
        this.temperature = temperature
    }
}

class SensorData {
    private lateinit var sensorName : String
    private var sensorId = -1
    private lateinit var temperatureData : ArrayList<TemperatureData>



    constructor(id: Int, name: String) {
        sensorId = id
        sensorName = name
    }

    fun addData(time: LocalDateTime, temperature : Float) {
        addData(TemperatureData(time, temperature))
    }

    fun addData(data : TemperatureData) {
        temperatureData.add(data)
    }

    fun setData(data: ArrayList<TemperatureData>) {
        temperatureData = data
    }

    fun getData(): ArrayList<TemperatureData> {
        return temperatureData
    }

    fun getName() : String {
        return sensorName
    }

    fun getId() : Int {
        return sensorId
    }
}