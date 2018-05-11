using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetManagerSystem
{
	/// <summary>
	// コルーチンの代理実行者インターフェース
	// </summary>
	public interface ICoroutineExecutor
	{
		void InvokeCoroutine( IEnumerator _coroutine);
	}
}
