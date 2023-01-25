import cv2

class DuplicateDetector:

	def __init__(self, hash_size = 8):
		self.hashes = []
		self.hash_size = hash_size
	
	def check_duplicate(self, image):
		hash_value = self.dhash(image)
		
		if hash_value in self.hashes:
			print("Found a duplicate")
			return True
		else:
			self.hashes.append(hash_value)
			return False
	
	def dhash(self, image):
		# convert image to grayscale and resize, adding one additional column width
		gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
		resized = cv2.resize(gray,(self.hash_size+1, self.hash_size))
		
		# compute horizontal gradient with respect to adjacent column pixels
		diff = resized[:,1:] > resized[:,:-1]
		
		# convert difference image to a hash and return
		return sum([2**i for (i,v) in enumerate(diff.flatten()) if v])
