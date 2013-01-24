using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Edit.AzureTableStorage
{
    /// <summary>
    /// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure storage services.
    /// 
    /// </summary>
    public class StorageTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public static readonly string[] TransientStorageErrorCodeStrings = new string[5]
    {
      "InternalError",
      "ServerBusy",
      "OperationTimedOut",
      "TableServerOutOfMemory",
      "TableBeingDeleted"
    };
        private static readonly Regex errorCodeRegex = new Regex(@"(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly int[] httpStatusCodes = new int[4]
    {
      500,
      504,
      503,
      408
    };
        private static readonly WebExceptionStatus[] webExceptionStatus = new WebExceptionStatus[9]
    {
      WebExceptionStatus.ConnectionClosed,
      WebExceptionStatus.Timeout,
      WebExceptionStatus.RequestCanceled,
      WebExceptionStatus.KeepAliveFailure,
      WebExceptionStatus.PipelineFailure,
      WebExceptionStatus.ReceiveFailure,
      WebExceptionStatus.ConnectFailure,
      WebExceptionStatus.SendFailure,
      WebExceptionStatus.NameResolutionFailure
    };
        private static readonly SocketError[] socketErrorCodes = new SocketError[2]
    {
      SocketError.ConnectionRefused,
      SocketError.TimedOut
    };

        static StorageTransientErrorDetectionStrategy()
        {
        }

        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// 
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>
        /// True if the specified exception is considered as transient, otherwise false.
        /// </returns>
        public bool IsTransient(Exception ex)
        {
            if (ex == null)
                return false;
            if (this.CheckIsTransient(ex))
                return true;
            if (ex.InnerException != null)
                return this.CheckIsTransient(ex.InnerException);
            else
                return false;
        }

        /// <summary>
        /// Checks whether the specified exception is transient.
        /// 
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>
        /// True if the specified exception is considered as transient, otherwise false.
        /// </returns>
        protected virtual bool CheckIsTransient(Exception ex)
        {
            WebException webException = ex as WebException;
            if (webException != null)
            {
                if (Enumerable.Contains<WebExceptionStatus>((IEnumerable<WebExceptionStatus>)StorageTransientErrorDetectionStrategy.webExceptionStatus, webException.Status))
                    return true;
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;
                    if (httpWebResponse != null && Enumerable.Contains<int>((IEnumerable<int>)StorageTransientErrorDetectionStrategy.httpStatusCodes, (int)httpWebResponse.StatusCode))
                        return true;
                }
                return false;
            }
            else
            {
                SocketException socketException = ex as SocketException;
                if (socketException != null && Enumerable.Contains<SocketError>((IEnumerable<SocketError>)StorageTransientErrorDetectionStrategy.socketErrorCodes, socketException.SocketErrorCode))
                    return true;
                DataServiceRequestException ex1 = ex as DataServiceRequestException;
                if (ex1 != null)
                {
                    if (Enumerable.Contains<string>((IEnumerable<string>)StorageTransientErrorDetectionStrategy.TransientStorageErrorCodeStrings, StorageTransientErrorDetectionStrategy.GetErrorCode(ex1)))
                        return true;
                    DataServiceResponse response = ex1.Response;
                    if (response != null && Enumerable.Any<OperationResponse>((IEnumerable<OperationResponse>)response, (Func<OperationResponse, bool>)(x => Enumerable.Contains<int>((IEnumerable<int>)StorageTransientErrorDetectionStrategy.httpStatusCodes, x.StatusCode))))
                        return true;
                }
                DataServiceClientException serviceClientException = ex as DataServiceClientException;
                return serviceClientException != null && Enumerable.Contains<int>((IEnumerable<int>)StorageTransientErrorDetectionStrategy.httpStatusCodes, serviceClientException.StatusCode) || (StorageTransientErrorDetectionStrategy.StorageV2ExceptionChecker.IsTransient(ex)) || (ex is TimeoutException || ex is IOException);
            }
        }

        protected static string GetErrorCode(DataServiceRequestException ex)
        {
            if (ex != null && ex.InnerException != null)
                return StorageTransientErrorDetectionStrategy.errorCodeRegex.Match(ex.InnerException.Message).Groups[1].Value;
            else
                return (string)null;
        }

        /// <summary>
        /// Provides support for checking transient faults when using the Azure Storage v2 managed API.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// This type avoids externalizing the usage of the Azure Storage assembly, so that if the application
        ///             is using another version of the assembly, then this type does not throw exceptions when the JIT
        ///             compiler tries to load this version of the assembly.
        /// 
        /// </remarks>
        private class StorageV2ExceptionChecker
        {
            public static bool IsTransient(Exception ex)
            {
                if (ex.GetType().FullName == "Microsoft.WindowsAzure.Storage.StorageException")
                    return StorageTransientErrorDetectionStrategy.StorageV2ExceptionChecker.IsTransientInternal(ex);
                else
                    return false;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static bool IsTransientInternal(Exception ex)
            {
                Microsoft.WindowsAzure.Storage.StorageException storageException = ex as Microsoft.WindowsAzure.Storage.StorageException;
                if (storageException != null)
                {
                    RequestResult requestInformation = storageException.RequestInformation;
                    if (requestInformation != null && (Enumerable.Contains<int>((IEnumerable<int>)StorageTransientErrorDetectionStrategy.httpStatusCodes, storageException.RequestInformation.HttpStatusCode) || requestInformation.ExtendedErrorInformation != null && Enumerable.Contains<string>((IEnumerable<string>)StorageTransientErrorDetectionStrategy.TransientStorageErrorCodeStrings, requestInformation.ExtendedErrorInformation.ErrorCode)))
                        return true;
                }
                return false;
            }
        }
    }
}
