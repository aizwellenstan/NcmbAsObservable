﻿using NCMB;
using System.Collections.Generic;
using UniRx;

namespace NcmbAsObservables
{
    /// <summary>
    /// NCMBScriptの拡張
    /// </summary>
    public static class NcmbScriptExtensions
    {
        /// <summary>
        /// 非同期処理でスクリプトの実行を行います。
        /// </summary>
        /// <param name="header">リクエストヘッダー.</param>
        /// <param name="body">リクエストボディ</param>
        /// <param name="query">クエリパラメーター</param>
        /// <returns>結果</returns>
        public static IObservable<byte[]> ExecuteAsyncAsStream(this NCMBScript script,
            IDictionary<string, object> header, IDictionary<string, object> body, IDictionary<string, object> query)
        {
            return Observable.Create<byte[]>(observer =>
            {
                var gate = new object();
                var isDisposed = false;
                script.ExecuteAsync(header, body, query, (data, error) =>
                {
                    lock (gate)
                    {
                        if (isDisposed) return;

                        if (error == null)
                        {
                            observer.OnNext(data);
                            observer.OnCompleted();
                        }
                        else
                        {
                            observer.OnError(error);
                        }
                    }
                });

                return Disposable.Create(() =>
                {
                    lock (gate)
                    {
                        isDisposed = true;
                    }
                });
            });
        }
    }
}
