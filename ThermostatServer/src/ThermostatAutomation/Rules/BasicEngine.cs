/*
 * This is an example of a very basic engine for the Thermostat. It's goal is to always keep the
 * same temperature in the selected zone/room.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class BasicEngine : IRulesEngine
    {
        //how many minutes the data is considered fresh
        private const double MinutesFresh = 5;
        //never go above 28 deg C
        private const decimal FailSafeTempCutOff = 28;

        public bool Evaluate(int channel)
        {
            //check that we have a last status for the selected room
            Zone selectedRoom = Status.Instance.Zones.SingleOrDefault(x => x.Name == Status.Instance.TargetZone);
            if (selectedRoom != null)
            {
                //check that we have a temperature for the target room, that we have a timestamp for that temperature and
                //that the timestamp is no older than 5 minutes
                if (selectedRoom.Temperature.HasValue && 
                    selectedRoom.Timestamp.HasValue &&
                    selectedRoom.Timestamp.Value >= DateTime.Now.AddMinutes(-MinutesFresh))
                {
                    if (selectedRoom.Temperature.Value < Status.Instance.TargetTemperature)
                    {
                        return true;
                    }
                    else
                    {
                        return false;   
                    }
                }
                else
                {
                    //TODO: notify of the failure

                    //will just have to use the highest temperature out of the ones that are fresh, 
                    //otherwise consider that the system has a failure and stop heating to prevent overheating
                    selectedRoom = Status.Instance.Zones.Where(x => x.Timestamp.HasValue &&
                        x.Timestamp >= DateTime.Now.AddMinutes(-MinutesFresh) &&
                        x.Temperature.HasValue)
                        .OrderByDescending(x => x.Temperature)
                        .FirstOrDefault();

                    if (selectedRoom != null)
                    {
                        if (selectedRoom.Temperature.Value < Status.Instance.TargetTemperature - 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            
            //TODO: notify of the failure

            //if we got here then things went wrong... avoid starting the heating
            return false;
        }
    }
}
