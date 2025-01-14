﻿using UnityEngine;

namespace _Barking_Dog.Common_Scripts
{
	public class Turn_Move : MonoBehaviour {
		public int TurnX;
		public int TurnY;
		public int TurnZ;

		public int MoveX;
		public int MoveY;
		public int MoveZ;

		public bool World;

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
			if (World == true) {
				transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.World);
				transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.World);
			}else{
				transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.Self);
				transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.Self);
			}
		}
	}
}
