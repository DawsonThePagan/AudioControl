using System;
using System.Linq;

namespace AudioControl
{
	internal class Program
	{
		const string VERSION = "V1.0";

		// Return codes
		const int RET_OK = 0;
		const int RET_ERR_EXTERN = 1; // External failure, i.e. audio api
		const int RET_ERR_USER = 2; // User error, i.e. invalid arguments

		// Arguments
		const string GET_DEFAULT = "/GETDEFAULT";
		const string GET_ALL = "/GETALL";
		const string GET_VOLUME = "/GETVOLUME";
		const string SET_DEFAULT = "/SETDEFAULT";
		const string SET_VOLUME = "/SETVOLUME";
		const string HELP_ARG = "/HELP";

		const string ARG_START_GET = "/GET";
		const string ARG_START_SET = "/SET";

		static int Main(string[] args)
		{
			if (args.Count() == 0 || args.Count() > 3)
			{
				Console.WriteLine("Invalid arguments provided, wrong number of arguments");
				return RET_ERR_USER;
			}

			string action;
			string value;
			string device = null;
			// If its a set we need the value, otherwise we don't
			try
			{
				if (args[0].ToUpper().StartsWith(ARG_START_SET))
				{
					action = args[0].Trim();
					value = args[1].Trim();
					if (args.Length > 2)
					{
						device = args[2].Trim();
					}
				}
				else if(args[0].ToUpper().StartsWith(ARG_START_GET))
				{
					action = args[0];
					value = "";
                    if (args.Length > 1)
                    {
                        device = args[1].Trim();
                    }
                }
				else if (args[0].ToUpper() == HELP_ARG)
				{
					Console.WriteLine($"=== AudioControl {VERSION} ===");
					Console.WriteLine("Allows easy control of windows audio devices from a command line.");
					Console.WriteLine(@"Return Codes:
  0 = Successful
  1 = Error, external. Windows has passed back an error
  2 = Error, user. Invalid arguments

Arguments:
All arguments are non case sensitive.
  /GetDefault = Get the default audio output device
  /GetAll = Get all connected audio devices
  /GetVolume = Get current volume level of default audio device
  /SetDefault ""<Device name>"" = Set the default audio device
  /SetVolume <Level> ""{Device}"" = Set the volume level of the default audio device, or any device by setting device
Be careful when setting default. Its suggested to run /GetAll first to double check your device name is correct

Developed by Bailey-Tyreese Dawson as part of BatchExtensions
Licensed under MIT License");
					return RET_OK;
				}
				else
				{
					Console.WriteLine("Invalid arguments provided");
					return RET_ERR_USER;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Argument provided are misformed. " + ex.Message);
				return RET_ERR_USER;
			}

			switch (action.ToUpper().Trim())
			{
				case SET_DEFAULT:
					bool success = false;
					try
					{
						var controller = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();
						foreach (var item in controller.GetPlaybackDevices())
						{
							if (item.State == AudioSwitcher.AudioApi.DeviceState.Disabled && item.State == AudioSwitcher.AudioApi.DeviceState.Unplugged && item.State == AudioSwitcher.AudioApi.DeviceState.NotPresent)
								continue;

							if (item.FullName == value)
							{
								success = true;
								controller.DefaultPlaybackDevice = item;
								break;
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception thrown, thrown while changing default. " + ex.Message);
						return RET_ERR_EXTERN;
					}

					if (!success)
					{
						Console.WriteLine("Could not find the audio device specified");
						return RET_ERR_USER;
					}

					Console.WriteLine("Successfully changed default audio device");
					break;

				case GET_DEFAULT:
					try
					{
						var controller = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();
						Console.WriteLine("Default audio device full name is '" + controller.DefaultPlaybackDevice.FullName + "'");
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception thrown while getting default. " + ex.Message);
						return RET_ERR_EXTERN;
					}
					break;

				case GET_VOLUME:
					try
					{
						var controller = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();

						if (device == null)
						{
							Console.WriteLine("Volume level is " + controller.DefaultPlaybackDevice.Volume);
						}
						else
						{
							bool found = false;
                            foreach (var item in controller.GetPlaybackDevices())
                            {
                                if (item.State == AudioSwitcher.AudioApi.DeviceState.Disabled && item.State == AudioSwitcher.AudioApi.DeviceState.Unplugged && item.State == AudioSwitcher.AudioApi.DeviceState.NotPresent)
                                    continue;

                                if (item.FullName == device)
                                {
                                    Console.WriteLine("Volume level is " + item.Volume);
									found = true;
                                    break;
                                }
                            }

							if(!found)
							{
								Console.WriteLine("Could not find device requested");
							}
                        }
					}
					catch (Exception ex)
					{
						Console.WriteLine("Failed to get the volume. " + ex.Message);
						return RET_ERR_EXTERN;
					}
					break;

				case GET_ALL:
					try
					{
						var controller = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();
						foreach (var item in controller.GetDevices())
						{
							if (item.State != AudioSwitcher.AudioApi.DeviceState.Disabled && item.State != AudioSwitcher.AudioApi.DeviceState.Unplugged && item.State != AudioSwitcher.AudioApi.DeviceState.NotPresent)
								Console.WriteLine("Name: '" + item.FullName + "' | Type: " + item.DeviceType + " | Volume: " + item.Volume);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Failed to all device names. " + ex.Message);
						return RET_ERR_EXTERN;
					}
					break;

				case SET_VOLUME:
					try
					{
						var controller = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController();
						Double value_converted = double.Parse(value);
						if (value_converted < 0 || value_converted > 100)
						{
							Console.WriteLine("Invalid volume level provided, must be between 0 and 100 inclusive.");
							return RET_ERR_USER;
						}

						if (device == null)
						{
							controller.DefaultPlaybackDevice.Volume = double.Parse(value);
						}
                        else
                        {
							bool found = false;
                            foreach (var item in controller.GetPlaybackDevices())
                            {
                                if (item.State == AudioSwitcher.AudioApi.DeviceState.Disabled && item.State == AudioSwitcher.AudioApi.DeviceState.Unplugged && item.State == AudioSwitcher.AudioApi.DeviceState.NotPresent)
                                    continue;

                                if (item.FullName == device)
                                {
                                    item.Volume = value_converted;
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                Console.WriteLine("Could not find device requested");
                            }
                        }
                    }
					catch (Exception ex)
					{
						Console.WriteLine("Failed to set the volume. " + ex.Message);
						return RET_ERR_EXTERN;
					}
					Console.WriteLine("Successfully set volume level");
					break;
				default:
					Console.WriteLine("Invalid argument, argument '" + action + "' is not an accepted argument.");
					return RET_ERR_USER;
			}

			return RET_OK;
		}
	}
}
