package com.philippgeil.smarthomeapp

import android.os.Build
import android.os.Bundle
import android.util.JsonReader
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import androidx.annotation.RequiresApi
import androidx.navigation.findNavController
import androidx.navigation.fragment.findNavController
import com.android.volley.Request
import com.android.volley.Response
import com.android.volley.toolbox.JsonArrayRequest
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.google.android.material.bottomnavigation.BottomNavigationMenuView
import com.google.android.material.bottomnavigation.BottomNavigationView
import com.google.android.material.card.MaterialCardView
import org.json.JSONArray
import org.json.JSONObject
import java.time.LocalDate
import java.time.LocalDateTime
import java.util.ArrayList

/**
 * A simple [Fragment] subclass as the default destination in the navigation.
 */
class HomeFragment : Fragment() {

    override fun onCreateView(
            inflater: LayoutInflater, container: ViewGroup?,
            savedInstanceState: Bundle?
    ): View? {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_home, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

//        val temperatureCardLayout = view.findViewById<LinearLayout>(R.id.temperature_card_layout);
//
//        val temperatureView = TextView(this.context)
//        temperatureView.text="Zimmer: 23°C"
//
//        temperatureCardLayout.addView(temperatureView)


        view.findViewById<MaterialCardView>(R.id.home_card_view).setOnClickListener{
            view.rootView.findViewById<BottomNavigationView>(R.id.bottom_nav_view).selectedItemId = R.id.page_temperature_data
            view.findNavController().navigate(R.id.action_home_fragment_to_TemperatureDataFragment)
        }

        val temperatureCardLayout = view.findViewById<LinearLayout>(R.id.temperature_card_layout)

    }

    @RequiresApi(Build.VERSION_CODES.O)
    fun getClimateData() : List<SensorData> {
        val queue = Volley.newRequestQueue(this.context)
        val dataUrl = "https://raspberrypi/api/climate/latest"
        val sensorUrl = "https://raspberrypi/api/climate/sensors"
        val data = ArrayList<SensorData>()

        val sensorRequest = JsonArrayRequest(Request.Method.GET, sensorUrl, null,
            Response.Listener { response -> {
                for (i in 0 until response.length()) {
                    val obj = response.getJSONObject(i)
                    val name = obj.getString("name")
                    val id = obj.getInt("id")

                    data.add(SensorData(id, name))
                }
            }},
            Response.ErrorListener {

            })

        val dataRequest = JsonArrayRequest(Request.Method.GET, dataUrl, null,
        Response.Listener { response -> {
            for (i in 0 until response.length()) {
                val obj = response.getJSONObject(i)
                val time = LocalDateTime.parse(obj.getString("name"))
                val temperature = obj.getDouble("temperature").toFloat()
                val sensorId = obj.getInt("sensorId")

                var sensor = data.find { sensor -> sensor.getId() == id }
                sensor?.addData(time, temperature)
            }
        }},
        Response.ErrorListener {  })

        queue.add(sensorRequest)
        queue.add(dataRequest)

        queue.start()

        return data
    }
}