using System;
using System.Collections.Generic;
using System.Text;

namespace kornsalad
{
    /*
     * fields = [
            'effect',
            'cut',
            'image_properties',
            'project',
            'parameters',
            'created_at'
        ]

        {"effect": 1, "cut": 67, "image_properties": "{}", "project": 2, "parameters": "20,2,2", "created_at": "2019-12-10T01:37:06.017609Z"}
     */
    class Task
    {
        public string effect { get; set; }
        public string parameters { get; set; }
        public string cut { get; set; }
        public string image_properties { get; set; } // canvas only
        public int project { get; set; }
        public string created_at { get; set; }
    }

    class RpcRequest
    {
        public Task[] Task { get; set; }
    }

    class RpcResponse
    {
        public bool success { get; set; }
        public string result { get; set; } // base64 encoded zip file
    }
}
