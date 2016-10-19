using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8

namespace omniata
{
    /**
     * Storage contains the operations to read and
     * write and initialize the persistent storage 
     * of the events.
     */
    public class Storage
    {
        private const string TAG = "Storage";

        /**
         * The fileformat is (just one line):
         * urlEncode(serialized_event)&urlEncode(serialized_event)&urlEncode(serialized_event)....
         * 
         * '&' works as a separator since it cannot be present in urlEncode(serialized_event)-entries.
         * The entries are 1st urlencoded to separated one from another, then
         * each event contains meta and data and because of that they are urlencoded and
         * finally each date and meta key and value is urlencoded.
         */
        private string DataFile { get; set; }
		private string DiscardedDataFile { get; set; }

        public Storage(string persistentDataPath)
        {
            DataFile = persistentDataPath + "/omniata-events.txt";
			DiscardedDataFile = persistentDataPath + "/omniata-discarded.txt";
		}

        public List<QueueElement> Read()
        {
            if (!Exist())
            {
                Log.Debug(TAG, "queue file missing");
                return new List<QueueElement>();
            }
            List<QueueElement> elements = new List<QueueElement>();
            string serialized = ReadFromFile();
            if (!String.IsNullOrEmpty(serialized) && serialized.Length > 0)
            {
                string[] elementsSerialized = serialized.Split('&');

                foreach (string elementSerialized in elementsSerialized)
                {
                    string data = WWW.UnEscapeURL(elementSerialized);
                    QueueElement queueElement = QueueElement.Deserialize(data);
                    if (queueElement == null)
                    {
                        Log.Error(TAG, "Invalid event found, skipping: " + data);
                    } else
                    {
                        elements.Add(queueElement);
                    }
                }
            }

            return elements;
        }

        public void Write(List<QueueElement> elements)
        {
            List<string> serializedElements = new List<string>();
            foreach (QueueElement element in elements)
            {
                serializedElements.Add(WWW.EscapeURL(element.Serialize()));
            }
            string serialized = String.Join("&", serializedElements.ToArray());

            WriteToFile(serialized);
        }

		public int ReadDiscarded()
		{
			if (!DiscardedExist()) {
				return 0;
			} else {
				return ReadFromDiscardedFile ();
			}
		}

		public void WriteDiscarded(int data)
		{
			WriteToDiscardedFile(data);
		}

        private string ReadFromFile()
        {
            StreamReader fileReader = new StreamReader(DataFile);            
            string data = fileReader.ReadLine();
            fileReader.Close();
            return data;
        }

        private void WriteToFile(string data)
        {
#if !(UNITY_WEBPLAYER || UNITY_WEBGL)
            StreamWriter fileWriter = File.CreateText(DataFile);          
            if (fileWriter == null)
            {
                Log.Error(TAG, "Creating file failed: " + DataFile);
                return;
            }
            fileWriter.WriteLine(data);
            fileWriter.Close();
#endif
        }

		private bool Exist()
		{
			return File.Exists(DataFile);
		}

		private int ReadFromDiscardedFile()
		{
			StreamReader fileReader = new StreamReader(DiscardedDataFile);            
			string data = fileReader.ReadLine();
			fileReader.Close();

			Log.Debug (TAG, "data: " + data);
			return Convert.ToInt32 (data.Trim());
		}
		
		private void WriteToDiscardedFile(int data)
		{
#if !(UNITY_WEBPLAYER || UNITY_WEBGL)
			StreamWriter fileWriter = File.CreateText(DiscardedDataFile);          
			if (fileWriter == null)
			{ 
				Log.Error(TAG, "Creating file failed: " + DiscardedDataFile);
				return;
			}
			fileWriter.WriteLine("" + data);
			fileWriter.Close();
#endif
		}

		private bool DiscardedExist()
		{
			return File.Exists(DiscardedDataFile);
		}
    }
}

#endif