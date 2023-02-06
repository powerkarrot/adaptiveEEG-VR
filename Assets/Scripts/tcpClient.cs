using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


public class tcpClient : MonoBehaviour
{
	#region private members
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	[SerializeField]
	private AdaptiveEDA adaptiveEDA;
	#endregion

	public double timeout = 1.0;
	public float NewAverage { get; private set; }

	//int i = 0;

	// Use this for initialization 	
	public void Awake()
	{
		socketConnection = new TcpClient("localhost", 8052);
	}

    /// <summary> 	
    /// Send message to server using socket connection. 	
    /// </summary> 	
    public new String SendMessage(string clientMessage)
	{
		print("Called " + clientMessage);
		if (socketConnection == null)
		{
			Debug.Log("error");
			return null;
		}
		try
		{
			// Get a stream object for writing.
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(clientMessage);
				//byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				//Debug.Log(clientMessageAsByteArray.Length);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			}

			var valueList = new List<float>();


			double time = UnixTime.GetTime();
			// Listen to new response
			while (!stream.DataAvailable & timeout < UnixTime.GetTime() - time)
			{
				if (socketConnection == null)
				{
					Debug.Log("socket interruption");
					break;
				}
			}

			Byte[] bytes = new Byte[1024];
			StringBuilder serverMessage = new StringBuilder();
			int length;
			// Read incomming stream into byte arrary. 	
			do
			{
				length = stream.Read(bytes, 0, bytes.Length);
				serverMessage.AppendFormat("{0}", Encoding.UTF8.GetString(bytes, 0, length));
			} while (stream.DataAvailable);

			Debug.Log("server message received as: " + serverMessage);

			/*if (serverMessage.Equals("{\"error\":\"not ready\"}") || serverMessage.Equals("{\"error\":\"no data\"}"))
            {
				return serverMessage.ToString();
            }
            else
            {
				SlopeData slopeData = JsonUtility.FromJson<SlopeData>(serverMessage.ToString());
				//string[] array = JsonUtility.FromJson<string[]>(serverMessage.ToString());
				adaptiveEDA.slope3min = slopeData.slope3min;
				adaptiveEDA.slope1min = slopeData.slope1min;
				Debug.Log(adaptiveEDA.slope1min);
			}
			// Converts the average string to floating point number
			//NewAverage = float.Parse(serverMessage.ToString());
			/*var listStr = serverMessage.ToString().Split('_');

			foreach (var value in listStr)
			{
				valueList.Add(float.Parse(value));
			}
			return valueList;*/
			

			return serverMessage.ToString();
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
			return "{\"error\":\"SocketException\"}";
		}
	}

	public void SendMessageNoReturn(string clientMessage)
	{
		if (socketConnection == null)
		{
			Debug.Log("error");
			return;
		}
		try
		{
			// Get a stream object for writing.
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			}

		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
			return;
		}
	}

	
}
