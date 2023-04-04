from typing import Optional

from DUE.Const import MAX_IO

import time


class SpiController:
    def __init__(self, serialPort):
        self.serialPort = serialPort

    def Write(self, dataWrite: bytes, chipselect: int = -1) -> bool:
        return self.WriteRead(dataWrite, 0, len(dataWrite), None, 0, 0, chipselect)

    def Read(self, dataRead: bytearray, chipselect: int = -1) -> bool:
        return self.WriteRead(None, 0, 0, dataRead, 0, len(dataRead), chipselect)

    def WriteRead(self, dataWrite: Optional[bytes], offsetWrite: int, countWrite: int,
                  dataRead: Optional[bytearray], offsetRead: int, countRead: int,
                  chipselect: int = -1) -> bool:
        if chipselect >= MAX_IO:
            raise ValueError("InvalidPin")

        if (dataWrite is None and dataRead is None) or (countWrite == 0 and countRead == 0):
            raise ValueError("Invalid arguments")

        if dataWrite is not None and offsetWrite + countWrite > len(dataWrite):
            raise ValueError("Invalid arguments")

        if dataRead is not None and offsetRead + countRead > len(dataRead):
            raise ValueError("Invalid arguments")

        if chipselect < 0:
            chipselect = 255

        cmd = f"spistream({countWrite},{countRead},{chipselect})"
        self.serialPort.WriteCommand(cmd)

        res = self.serialPort.ReadRespone()

        if not res.success:
            return False

        transaction_step = 0
        while countWrite > 0 or countRead > 0:
            if countWrite > 0:
                self.serialPort.WriteRawData(dataWrite, offsetWrite, 1)
                offsetWrite += 1
                countWrite -= 1

            if countRead > 0:
                self.serialPort.ReadRawData(dataRead, offsetRead, 1)
                offsetRead += 1
                countRead -= 1

            transaction_step += 1

            if transaction_step % self.serialPort.TransferBlockSizeMax == 0:
                # each block needs delay for the device vcom get all data
                time.sleep(self.serialPort.TransferBlockDelay)

        res = self.serialPort.ReadRespone()
        return res.success
    
    def Write4bpp(self, dataWrite: Optional[bytes], offsetWrite: int, countWrite: int,  chipselect: int = -1) -> bool:   
        if chipselect >= MAX_IO:
            raise ValueError("InvalidPin")
        
        if (dataWrite is None ) or (countWrite == 0):
            raise ValueError("Invalid arguments")
        
        cmd = f"spi4bpp({countWrite},{chipselect})"
        self.serialPort.WriteCommand(cmd)

        res = self.serialPort.ReadRespone()

        if not res.success:
            return False

        self.serialPort.WriteRawData(dataWrite, offsetWrite, countWrite)

        res = self.serialPort.ReadRespone()
        return res.success
    
    def Pallete(self, id:int, color: int) -> bool:
        if id > 16:
            raise ValueError("Pallete supports 16 color index only.")
        
        cmd = f"palette({id},{color})"

        self.serialPort.WriteCommand(cmd)

        res = self.serialPort.ReadRespone()
        return res.success
    
    def Configuration(self,mode: int,  frequencyKHz: int)-> bool:
        if mode > 3 or mode < 0:
            raise ValueError("Mode must be in range 0...3.")
        
        if frequencyKHz < 200  or frequencyKHz > 20000:
            raise ValueError("FrequencyKHz must be in range 200KHz to 20MHz.")
        
        cmd = f"palette({mode},{frequencyKHz})"

        self.serialPort.WriteCommand(cmd)

        res = self.serialPort.ReadRespone()
        return res.success
    
