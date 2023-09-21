import matplotlib
matplotlib.use("TkAgg")
import matplotlib.pyplot as plt

import os
import io
import scipy.misc
import numpy as np
from six import BytesIO
from PIL import Image, ImageDraw, ImageFont

import tensorflow as tf

from object_detection.utils import label_map_util
from object_detection.utils import config_util
from object_detection.utils import visualization_utils as viz_utils
from object_detection.builders import model_builder

def load_image_into_numpy_array(path):
  """Load an image from file into a numpy array.

  Puts image into numpy array to feed into tensorflow graph.
  Note that by convention we put it into a numpy array with shape
  (height, width, channels), where channels=3 for RGB.

  Args:
    path: the file path to the image

  Returns:
    uint8 numpy array with shape (img_height, img_width, 3)
  """
  img_data = tf.io.gfile.GFile(path, 'rb').read()
  image = Image.open(BytesIO(img_data))
  (im_width, im_height) = image.size
  return np.array(image.getdata()).reshape(
      (im_height, im_width, 3)).astype(np.uint8)

def get_keypoint_tuples(eval_config):
  """Return a tuple list of keypoint edges from the eval config.
  
  Args:
    eval_config: an eval config containing the keypoint edges
  
  Returns:
    a list of edge tuples, each in the format (start, end)
  """
  tuple_list = []
  kp_list = eval_config.keypoint_edge
  for edge in kp_list:
    tuple_list.append((edge.start, edge.end))
  return tuple_list

#checkpoint "./RPN/faster_rcnn_resnet50_v1_640x640_coco17_tpu-8/checkpoint/ckpt-0"

pipeline_config = "./configs/frcnn_resnet50.config"
#pipeline_config = "C:/Users/svcwe/Desktop/adjustedRCNN/RPN/faster_rcnn_resnet50_v1_640x640_coco17_tpu-8/pipeline.config"
model_dir = "C:/Users/svcwe/Desktop/adjustedRCNN/trainingcheckpoints"
#model_dir = r"C:\Users\svcwe\Desktop\adjustedRCNN\RPN\faster_rcnn_resnet50_v1_640x640_coco17_tpu-8\checkpoint"
# Load pipeline config and build a detection model
configs = config_util.get_configs_from_pipeline_file(pipeline_config)
model_config = configs['model']
detection_model = model_builder.build(
      model_config=model_config, is_training=False)

# Restore checkpoint
ckpt = tf.compat.v2.train.Checkpoint(
      model=detection_model)
ckpt.restore(os.path.join(model_dir, 'ckpt-5')).expect_partial()

#def get_model_detection_function(model):
#  """Get a tf.function for detection."""

#  @tf.function
def detect_fn(image):
  """Detect objects in image."""
  image, shapes = detection_model.preprocess(image)
  prediction_dict = detection_model.predict(image, shapes)
  detections = detection_model.postprocess(prediction_dict, shapes)

  return detections, prediction_dict, tf.reshape(shapes, [-1])

#  return detect_fn

#detect_fn = get_model_detection_function(detection_model)

# label map
label_map_path = configs['eval_input_config'].label_map_path
label_map = label_map_util.load_labelmap(label_map_path)
categories = label_map_util.convert_label_map_to_categories(
    label_map,
    max_num_classes=label_map_util.get_max_label_map_index(label_map),
    use_display_name=True)
category_index = label_map_util.create_category_index(categories)
label_map_dict = label_map_util.get_label_map_dict(label_map, use_display_name=True)

#image_dir = './object_detection/test_images/'
#image_path = os.path.join(image_dir, 'image3.jpg')
#image_dir = 'eval/'
#image_path = os.path.join(image_dir, '200_color.png')
image_dir = 'eval/synthetic_images'
image_path = os.path.join(image_dir, 'span_00001.jpg')
#image_dir = 'raccoon/images/'
#image_path = os.path.join(image_dir, 'raccoon-117.jpg')

image_np = load_image_into_numpy_array(image_path)

# Things to try:
# Flip horizontally
# image_np = np.fliplr(image_np).copy()

# Convert image to grayscale
# image_np = np.tile(
#     np.mean(image_np, 2, keepdims=True), (1, 1, 3)).astype(np.uint8)

input_tensor = tf.convert_to_tensor(
    np.expand_dims(image_np, 0), dtype=tf.float32)
detections, predictions_dict, shapes = detect_fn(input_tensor)
print("\n\n", len(detections['detection_boxes'][0]), "\n\n")
print("\n\n", detections['detection_scores'][0][0:4].numpy(), "\n\n")
print("\n\n", detections['detection_classes'][0][0:4], "\n\n")

label_id_offset = 1
image_np_with_detections = image_np.copy()

# Use keypoints if available in detections
keypoints, keypoint_scores = None, None
if 'detection_keypoints' in detections:
  keypoints = detections['detection_keypoints'][0].numpy()
  keypoint_scores = detections['detection_keypoint_scores'][0].numpy()

viz_utils.visualize_boxes_and_labels_on_image_array(
      image_np_with_detections,
      detections['detection_boxes'][0].numpy(),
      (detections['detection_classes'][0].numpy() + label_id_offset).astype(int),
      detections['detection_scores'][0].numpy(),
      category_index,
      use_normalized_coordinates=True,
      max_boxes_to_draw=200,
      min_score_thresh=0.3,
      agnostic_mode=False,
      keypoints=keypoints,
      keypoint_scores=keypoint_scores,
      keypoint_edges=get_keypoint_tuples(configs['eval_config']))

plt.figure(figsize=(12,16))
plt.imshow(image_np_with_detections)
import cv2
cv2.imshow("image",image_np_with_detections)
cv2.waitKey(0)
#plt.show()
plt.savefig("figure.png")




"""
ap = argparse.ArgumentParser()
ap.add_argument("-i", "--image", required=True,
	help="path to input image")
args = vars(ap.parse_args())

img_data = tf.io.gfile.GFile(args["image"], 'rb').read()
image = Image.open(BytesIO(img_data))
(im_width, im_height) = image.size
image_np = np.array(image.getdata()).reshape(
	(im_height, im_width, 3)).astype(np.uint8)

from object_detection.utils import config_util
#from object_detection.utils import label_map_util, visualization_utils as viz_utils
from object_detection.builders import model_builder

pipeline_config = "C:/Users/svcwe/Desktop/adjustedRCNN/RPN/faster_rcnn_resnet50_v1_640x640_coco17_tpu-8/pipeline.config"
model_dir = "C:/Users/svcwe/Desktop/adjustedRCNN/RPN/faster_rcnn_resnet50_v1_640x640_coco17_tpu-8/checkpoint"

# load pipeline, build model
configs = config_util.get_configs_from_pipeline_file(pipeline_config)
model_config = configs['model']
detection_model = model_builder.build(
      model_config=model_config, is_training=False)

# restore checkpoint
ckpt = tf.compat.v2.train.Checkpoint(
      model=detection_model)
ckpt.restore(os.path.join(model_dir, 'ckpt-0')).expect_partial()

# get predicted ROIs
print("[INFO] running RPN...")
input_tensor = tf.convert_to_tensor(
    np.expand_dims(image_np, 0), dtype=tf.float32)

image_np, shapes = model.preprocess(image)
prediction_dict = model.predict(image, shapes)
detections = model.postprocess(prediction_dict, shapes)
shapes = tf.reshape(shapes, [-1])

label_id_offset = 1
#image_np_with_detections = image_np.copy()

# visualize (not sure for now)
rects = np.array(detections['detection_boxes'])
"""