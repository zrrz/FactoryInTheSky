using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//public enum BlockType { Air, Stone, Wood, Dirt, Leaves }

public class ChunkData {

    public static int CHUNK_SIZE = 16;
    private BlockStorage storage = new BlockStorage(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE);

    public void SetBlock(int x, int y, int z, int blockID)
    {
        //1D to 3D
        //int x = index & 0xf;
        //int y = (index >> 4) & 0xf;
        //int z = (index >> 8) & 0xf;

        int index = x | y << 4 | z << 8;
        //int index = x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z;
        storage.SetBlock(index, blockID);
    }

    public int GetBlock(int x, int y, int z)
    {
        int index = x | y << 4 | z << 8;
        //int index = x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z;
        return storage.GetBlock(index);
    }
}

// the actual storage for blocks
class BlockStorage
{
    private int size;
    private BitBuffer data;
    private PaletteEntry[] palette;
    private int paletteCount;
    private int indicesLength;

    public BlockStorage(int size)
    {
        this.size = size;
        this.data = new BitBuffer(size);
        this.palette = new PaletteEntry[2];
        this.palette[0].refcount = size;
        this.paletteCount = 1;
        this.indicesLength = 1;
    }

    public void SetBlock(int index, int blockID)
    {
        int paletteIndex = data.Get(index, indicesLength/*, true*/);
        if (paletteIndex < 0 || paletteIndex > palette.Length - 1)
        {
            Debug.LogError("paletteIndex: " + paletteIndex);
            return;
        }
        PaletteEntry current = palette[paletteIndex];

        // Whatever block is there will cease to exist in but a moment...
        current.refcount -= 1;

        // The following steps/choices *must* be ordered like they are.

        // --- Is the block-type already in the palette?
        int currentPaletteIndex = System.Array.FindIndex<PaletteEntry>(palette, entry => { return entry.blockID.Equals(blockID); });
        if (currentPaletteIndex != -1)
        {
            //Debug.LogError("Already in palette: " + type.ToString());
            // YES: Use the existing palette entry.
            data.Set(index, indicesLength, currentPaletteIndex);
            palette[currentPaletteIndex].refcount += 1;
            return;
        }

        // --- Can we overwrite the current palette entry?
        if (current.refcount == 0)
        {
            Debug.Log("Overwrite current palette entry");
            // YES, we can!
            current.blockID = blockID;
            current.refcount = 1;
            return;
        }

        Debug.Log("Add new Palette:" + blockID.ToString());
        // --- A new palette entry is needed!

        // Get the first free palette entry, possibly growing the palette!
        int newEntry = NewPaletteEntry();

        palette[newEntry] = new PaletteEntry { refcount = 1, blockID = blockID };
        data.Set(index, indicesLength, newEntry/*, true*/);
        paletteCount += 1;
    }

    public int GetBlock(int index)
    {
        int paletteIndex = data.Get(index, indicesLength);

        if (paletteIndex < 0 || paletteIndex > palette.Length - 1)
        {
            Debug.LogError("paletteIndex: " + paletteIndex);
            return 0;
        }

        return palette[paletteIndex].blockID;
    }

    private int NewPaletteEntry()
    {

        //Debug.LogError("Getting new Pallete entry");
        int firstFree = System.Array.FindIndex<PaletteEntry>(palette, (entry) => { return /*entry.blockType == BlockType.Air ||*/ entry.refcount == 0; });

        if (firstFree != -1)
        {
            //Debug.LogError("Giving pallete spot: " + firstFree);
            return firstFree;
        }

        // No free entry?
        // Grow the palette, and thus the BitBuffer
        GrowPalette();

        // Just try again now!
        return NewPaletteEntry();
    }

    private void GrowPalette()
    {
        // decode the indices
        int[] indices = new int[size];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = data.Get(i, indicesLength);
        }

        // Create new palette, doubling it in size
        indicesLength = indicesLength << 1;
        PaletteEntry[] newPalette = new PaletteEntry[indicesLength];
        System.Array.Copy(palette, 0, newPalette, 0, paletteCount);
        palette = newPalette;

        // Allocate new BitBuffer
        data = new BitBuffer(size * newPalette.Length); // this is bits, not bytes!
        Debug.Log("New Palette size: " + newPalette.Length);
        Debug.Log("New ByteBitBuffer size: " + size * newPalette.Length + " bytes");

        // Encode the indices
        for (int i = 0; i < indices.Length; i++)
        {
            data.Set(i, indicesLength, indices[i]);
        }
    }

    // Shrink the palette (and thus the BitBuffer) every now and then.
    // You may need to apply heuristics to determine when to do this.
    public void FitPalette()
    {
        // Remove old entries...
        for (int i = 0; i < palette.Length; i++)
        {
            if (palette[i].refcount == 0)
            {
                palette[i].blockID = 0;
                paletteCount -= 1;
            }
        }

        // Is the palette less than half of its closest power-of-two?
        if (paletteCount > Mathf.Pow(paletteCount, 2f) / 2)
        {
            // NO: The palette cannot be shrunk!
            return;
        }

        // decode all indices
        int[] indices = new int[size];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = data.Get(i, indicesLength);
        }

        // Create new palette, halfing it in size
        indicesLength = indicesLength >> 1;
        PaletteEntry[] newPalette = new PaletteEntry[Mathf.FloorToInt(Mathf.Pow(2f, (float)indicesLength))];

        // We gotta compress the palette entries!
        int paletteCounter = 0;
        for (int pi = 0; pi < palette.Length; pi++, paletteCounter++)
        {
            PaletteEntry entry = newPalette[paletteCounter] = palette[pi];

            // Re-encode the indices (find and replace; with limit)
            for (int di = 0, fc = 0; di < indices.Length && fc < entry.refcount; di++)
            {
                if (pi == indices[di])
                {
                    indices[di] = paletteCounter;
                    fc += 1;
                }
            }
        }

        // Allocate new BitBuffer
        data = new BitBuffer(size * newPalette.Length); // this is bits, not bytes!

        // Encode the indices
        for (int i = 0; i < indices.Length; i++)
        {
            data.Set(i, indicesLength, indices[i]);
        }
    }
}

struct PaletteEntry
{
    public int refcount;
    public int blockID;
}

public class BitBuffer
{
    /// <summary>
    /// The binary buffer.
    /// </summary>
    private readonly int[] data;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitBuffer"/> class.
    /// </summary>
    /// <param name="length">The length of the buffer in bits.</param>
    public BitBuffer(int length)
    {
        var len = length / 32;
        if (length % 32 != 0)
        {
            Debug.LogError("Size not divisible by 32: size " + length);
            //len++;
        }

        this.data = new int[len];
    }

    /// <summary>
    /// Sets the value at an index.
    /// </summary>
    /// <param name="index">The index (0 - chunk_size^3).</param>
    /// <param name="length">The length in bits.</param>
    /// <param name="value">The value to set.</param>
    public void Set(int index, int length, int value/*, bool log = false*/)
    {
        int idx = index / (32 / length);
        //Debug.LogError("index: " + index + " length: " + length + " intIndex: " + idx + "/" + data.Length);
        int intData = this.data[idx];
        int mask = ((1 << length) - 1);
        //if(log)
        //  Debug.LogError("Mask: " + Convert.ToString(mask, 2).PadLeft(32, '0'));

        int shift = ((index * length) % 32);

        mask <<= shift;
        //if (log)
        //  Debug.LogError("shift: " + shift + " - " + Convert.ToString(mask, 2).PadLeft(32, '0'));

        intData &= ~mask;
        //if (log)
        //  Debug.LogError("intData: " + Convert.ToString(intData, 2).PadLeft(32, '0'));

        value <<= shift;
        //if (log)
        //  Debug.LogError("value: " + Convert.ToString(value, 2).PadLeft(32, '0'));

        intData |= value;
        //if (log)
        //  Debug.LogError("Final: " + Convert.ToString(intData, 2).PadLeft(32, '0'));

        this.data[idx] = intData;
    }

    /// <summary>
    /// Gets the value at an index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The value.</returns>
    public int Get(int index, int length/*, bool log = false*/)
    {
        int intData = this.data[index / (32 / length)];
        //if(log)
        //  Debug.LogError("intData: " + Convert.ToString(intData, 2).PadLeft(32, '0'));
        int mask = (1 << length) - 1;
        //if(log)
        //  Debug.LogError("mask: " + Convert.ToString(mask, 2).PadLeft(32, '0'));
        int shift = ((index * length) % 32);

        //if(log)
        //  Debug.LogError("Final: " + Convert.ToString(((intData >> shift) & mask), 2).PadLeft(32, '0'));
        return ((intData >> shift) & mask);
    }
}