import h5py
import os

class HDF5DatasetWriter_Compressed:
	def __init__(self, dims, outPath, dataKey = "images", bufSize = 1000):
		# check if output path exists, and if so, raise an exception
		if os.path.exists(outPath):
			raise ValueError("The supplied `outPath` already exists and cannot be overwritten"
				"Manually delete the file before continuing", outPath)
			
		# open HDF5 database to write and create 2 datasets:
		# one to store images, other for class labels
		self.db = h5py.File(outPath, "w")
		self.data = self.db.create_dataset(dataKey, dims, dtype="int")
		self.labels = self.db.create_dataset("labels",(dims[0],), dtype="int")
		
		# store the buffer size, then initialize the buffer along with
		# the index into the datasets
		self.bufSize = bufSize
		self.buffer = {"data":[], "labels":[]}
		self.idx = 0
		
	def add(self, rows, labels):
		# add rows and labels to the buffer
		self.buffer["data"].extend(rows)
		self.buffer["labels"].extend(labels)
		
		# if buffer is full, flush the rows in buffer to disk
		if len(self.buffer['data']) >= self.bufSize:
			self.flush()
			
	def flush(self):
		# write buffers to disk and reset them
		i = self.idx + len(self.buffer["data"])
		self.data[self.idx:i] = self.buffer["data"]
		self.labels[self.idx:i] = self.buffer["labels"]
		self.idx = i
		self.buffer = {"data":[], "labels":[]}
		
	def storeClassLabels(self, classLabels):
		# create a dataset to store the actual class label names,
		# then store the class labels
		dt = h5py.special_dtype(vlen=unicode)
		labelSet = self.db.create_dataset("label_names", (len(classLabels),), dtype = dt)
		labelSet[:] = classLabels
		
		
	def close(self):
		# check to see if there are any other entries in the buffer 
		# that need to be flushed to the disk
		if len(self.buffer["data"]) > 0:
			self.flush()
			
		# close the dataset
		self.db.close()
		 
