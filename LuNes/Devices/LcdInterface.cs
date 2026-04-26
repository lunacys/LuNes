namespace LuNes.Devices;

public class LcdInterface
{
    private readonly W65C22 _via;
    private readonly HD44780 _lcd;

    private byte _lastPortA;
    private byte _lastPortB;
    
    public byte LastPortA => _lastPortA;
    public byte LastPortB => _lastPortB;

    public LcdInterface(W65C22 via, HD44780 lcd)
    {
        _via = via;
        _lcd = lcd;
    }

    public void Clock()
    {
        // Update busy timer and communicate busy state to VIA
        _lcd.UpdateBusy();
        _via.LcdBusy = _lcd.IsBusy;

        byte nowA = _via.PortALatch;
        byte nowB = _via.PortBLatch;

        // Detect rising edge of E (PA7)
        bool eRising = (nowA & 0x80) != 0 && (_lastPortA & 0x80) == 0;

        if (eRising)
        {
            bool rs = (nowA & 0x20) != 0;
            bool rw = (nowA & 0x40) != 0;

            if (!rw) // write
            {
                if (rs) _lcd.WriteData(nowB);
                else    _lcd.WriteCommand(nowB);
            }
            // Read (RW high) can be ignored for now
        }

        _lastPortA = nowA;
        _lastPortB = nowB;
    }
}