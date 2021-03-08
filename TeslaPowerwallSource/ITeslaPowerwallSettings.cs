using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaPowerwallSource
{
    public interface ITeslaPowerwallSettings
    {
        string? IP { get; set; }
        string Password { get; set; }
        string Email { get; set; }
    }
}
