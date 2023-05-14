namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Implements a Kalman filter algorithm for filtering noisy data.
    /// The filter takes in noisy measurements of a system's state and returns a filtered estimate of the true state.
    /// The filter uses a dynamic model of the system's behavior and the variance of the process noise and measurement noise to update the estimate.
    /// The filter can be used to reduce the impact of noise in real-time sensor data.
    /// </summary>
    public class KalmanFilter
    {
        /// <summary>
        /// Process noise variance.
        /// Represents the uncertainty in the system's dynamic model.
        /// </summary>
        private float Q;
        /// <summary>
        /// Measurement noise variance.
        /// Represents the uncertainty in the measured data.
        /// </summary>
        private float R;
        /// <summary>
        /// Estimate error covariance.
        /// Represents the uncertainty in the state estimate.
        /// </summary>
        private float P;
        /// <summary>
        /// Kalman gain.
        /// Determines the weight given to the measured data versus the predicted state estimate.
        /// </summary>
        private float K;
        /// <summary>
        /// State estimate.
        ///  The current estimate of the system state.
        /// </summary>
        private float X;
        /// <summary>
        /// Previous state estimate.
        /// The previous estimate of the system state, which is used to calculate the current estimate.
        /// </summary>
        private float prevX;
        /// <summary>
        /// Flag to track if the filter has been initialized
        /// </summary>
        private bool initialized;

        /// <summary>
        /// The KalmanFilter constructor initializes the filter with the variance of the process noise and measurement noise.
        /// A higher value for processNoise means that the filter trusts the dynamic model more.
        /// A higher value for measurementNoise means that the filter trusts the measured data more.
        /// </summary>
        /// <param name="processNoise">This parameter controls how much weight the filter gives to the predicted state versus the measured data.</param>
        /// <param name="measurementNoise">This parameter controls how much weight the filter gives to the measured data versus the predicted state.</param>
        public KalmanFilter(float processNoise, float measurementNoise)
        {
            Q = processNoise;
            R = measurementNoise;
            P = 1.0f;
            K = 0.0f;
            X = 0.0f;
            prevX = 0.0f;
            initialized = false;
        }

        /// <summary>
        /// Filters a measurement through the Kalman filter and returns the estimated state.
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public float Filter(float measurement)
        {
            if (!initialized)
            {
                // Seed the filter with its first value
                X = measurement;
                initialized = true;
            }
            else
            {
                // begin with our previous value
                X = prevX;
                // Add to error covariance
                P += Q;

                // Calculate Kalman Gain
                K = P / (P + R);
                // Update Estimate value
                X += K * (measurement - X);
                // Update error covariance
                P = (1 - K) * P;
            }
            // Save as previous state
            prevX = X;
            // Return predicted value
            return X;
        }
    }
}
