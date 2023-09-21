from tensorflow.keras.models import Model
from tensorflow.keras.layers import Average

class EnsembleSoftmax:
	
	def build(self,model_list = None, model_input = None):
		# check if output sizes are same


		
		# average the outputs
		outputs = [model.output for model in model_list]
		output = Average()(outputs)		


		# build the final model
		ensemble_model = Model ( model_input, output, name = 'ensemble')

		return ensemble_model
