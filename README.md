Hydrogen Engine Regulator
---

An in-game script for Space Engineers to manage the power of batteries and the
level of hydrogen in hydrogen tanks .

How this is calculated is via three variables:   
`minBatteryLevel`: The minimum battery level in order for the hydrogen engine to
turn on   
`maxBatteryLevel`: The maximum battery level in order for the hydrogen engine to
turn off   
`minHydrogenTankLevel`: The minimum hydrogen level in order for the script to
ignore turning on the hydrogen engine.   
These are calculated as floats and should be set to a number between zero and
one.
These are also percentages not values.

There is a range of activity that the script will have and that is defined by
the `minBatteryLevel` and `maxBatteryLevel`.
Just incase you are running out of hydrogen the `minHydrogenTankLevel`
variable will prevent any further use of hydrogen.

If you have any issues starting the engine, what you can do is go into the
"Custom Data" for the programable block and set the value of either
"Battery.Full" or "Battery.Empty" to the other.
