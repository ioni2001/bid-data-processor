namespace Big.Data.DataProcessor.Services;

public interface IPredictionService
{
    PredictionResponse Predict(PredictionRequest request);
}
