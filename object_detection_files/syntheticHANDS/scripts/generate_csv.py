import csv
import os


def count_to_class(count):
    if count == 1:
        return "eight"
    elif count == 2:
        return "five"
    elif count == 3:
        return "four"
    elif count == 4:
        return "nine"
    elif count == 5:
        return "one"
    elif count == 6:
        return "punch"
    elif count == 7:
        return "seven"
    elif count == 8:
        return "six"
    elif count == 9:
        return "span"
    elif count == 10:
        return "three"
    elif count == 11:
        return "two"
    


imgfilename = ""
width = "512"
height = "512"
imgclass = ""
finalrows = [["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"]]




model_list = [f.path for f in os.scandir(os.getcwd()) if f.is_dir()]
modelnum = 1
for path in model_list:
    finalrows = [["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"]]
    folders = [f.path for f in os.scandir(path) if f.is_dir()]
    count = 1
    for folder in folders:
        images = os.listdir(folder)
        imgclass = count_to_class(count)
        for img in images:
            finalrows.append([img, width, height, imgclass])
        count += 1
    
    modelnum += 1

with open('./model1.csv', mode='w') as csv_output:
        csv_writer = csv.writer(csv_output, delimiter=',', lineterminator='\n')
        
        csv_writer.writerows(finalrows)
        








"""
bboxes

model1 one: 333 139 379 220
model1 two: 334 136 382 223
model1 three: 320 136 379 221
model1 four: 331 135 385 224
model1 five: 320 136 385 222
model1 six: 336 136 383 222
model1 seven: 331 135 377 222
model1 eight: 331 141 376 225
model1 nine: 327 136 380 225
model1 punch: 332 168 379 222
model1 span: 329 163 395 222

model2 one: 339 134 388 228
model2 two: 341 130 389 226
model2 three: 322 129 386 227
model2 four: 337 129 394 226
model2 five: 322 130 393 225
model2 six: 342 130 390 227
model2 seven: 338 130 383 226
model2 eight: 336 136 384 226
model2 nine: 332 131 388 225
model2 punch: 336 170 386 225
model2 span: 332 161 404 227

model3 one: 346 131 395 218
model3 two: 348 130 396 222
model3 three: 333 130 393 219
model3 four: 345 129 401 220
model3 five: 332 130 400 220
model3 six: 350 130 398 220
model3 seven: 346 130 391 220
model3 eight: 345 136 390 220
model3 nine: 341 130 396 220
model3 punch: 344 164 394 220
model3 span: 343 158 411 220

model4 one: 354 139 402 221
model4 two: 356 134 402 221
model4 three: 342 135 399 221
model4 four: 351 135 407 221
model4 five: 342 134 406 221
model4 six: 357 135 404 221
model4 seven: 353 134 399 221
model4 eight: 353 142 397 221
model4 nine: 349 135 402 221
model4 punch: 352 168 400 221
model4 span: 351 162 417 221

model5 one: 351 147 398 225
model5 two: 353 143 399 225
model5 three: 340 142 396 225
model5 four: 351 143 403 225
model5 five: 339 144 401 225
model5 six: 354 144 401 225
model5 seven: 351 144 393 225
model5 eight: 350 150 393 225
model5 nine: 348 145 395 225
model5 punch: 351 176 397 225
model5 span: 346 170 410 225
"""