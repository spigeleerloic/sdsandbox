import os
import random
import json
import time
from io import BytesIO
import base64
import logging

import cv2
import threading
import tkinter
from test_client import SimpleClient


class windowInterface(tkinter.Tk):  # screen to display interface for ALL client
    def __init__(self, name="camera configuration interface"):
        super().__init__(name)


class CameraConfigClient(SimpleClient):

    def __init__(self, address, poll_socket_sleep_time=0.01):
        super().__init__(address, poll_socket_sleep_time=poll_socket_sleep_time, verbose=False)

        self.cam_config = {
            "msg_type": "cam_config",
            "fov": 90,
            "fish_eye_x": 0.4,
            "fish_eye_y": 0.7,
            "img_w": 160,
            "img_h": 120,
            "img_d": 3,
            "img_enc": "JPG",
            "offset_x": 0.0,
            "offset_y": 1.120395,
            "offset_z": 0.5528488,
            "rot_x": 15.0
        }  # default camera config

        self.default = self.cam_config

        self.resolutions = {
            "fov": (1, 0, 180, int),
            "fish_eye_x": (0.05, 0, 1, float),
            "fish_eye_y": (0.05, 0, 1, float),
            "img_w": (1, 64, 512, int),
            "img_h": (1, 64, 512, int),
            "img_d": (1, 1, 3, int),
            "offset_x": (0.1, -10, 10, float),
            "offset_y": (0.1, -10, 10, float),
            "offset_z": (0.1, -10, 10, float),
            "rot_x": (1.0, 0.0, 180.0, float)
        }  # resolution, min, max and type

        self.window = windowInterface()
        self.scales_value = []

        # create the sliders
        for it, key in enumerate(self.resolutions):
            value = tkinter.DoubleVar()
            step, min_, max_, _ = self.resolutions[key]

            s = tkinter.Scale(self.window, resolution=step,
                              variable=value, label=self.cam_config[key],
                              length=75, width=15,
                              from_=min_, to=max_,
                              command=self.update_slider_value)
            s.grid(row=0, column=it)
            self.scales_value.append(value)

        # set the sliders to a default value
        for it, key in enumerate(self.resolutions):
            self.scales_value[it].set(self.cam_config[key])

        # send camera config for the first time
        self.send_config()
        self.send_camera_config()

        # start the imshow thread to have the camera feedback
        threading.Thread(target=self.imgshow_thread).start()

    def update_slider_value(self, v=0):
        for key, scale_value in zip(self.resolutions, self.scales_value):
            data_type = self.resolutions[key][-1]
            self.cam_config[key] = data_type(scale_value.get())
        self.send_camera_config()

    def send_camera_config(self):
        # prepare the packet
        tmp_config = {}
        for key in self.cam_config:
            tmp_config[key] = str(self.cam_config[key])

        self.send_now(json.dumps(tmp_config))

    def imgshow_thread(self):
        while(True):
            if self.last_image is None:
                continue
            cv2.imshow('camera', self.last_image)
            cv2.waitKey(33)


def run_cam_client():

    host = "127.0.0.1"
    port = 9091

    c = CameraConfigClient(address=(host, port))

    # Load Scene message. Only one client needs to send the load scene.
    msg = '{ "msg_type" : "load_scene", "scene_name" : "generated_track" }'
    c.send_now(msg)
    c.window.mainloop()


if __name__ == "__main__":
    run_cam_client()
