using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// A memory arena allocator that provides fast, linear memory allocation without triggering garbage collection.
/// 
/// This allocator works by pre-allocating a large contiguous block of unmanaged memory and then 
/// handing out sequential chunks of that memory as requested. This approach offers several benefits:
/// - No garbage collection pressure since all memory is unmanaged
/// - Very fast allocation (just pointer arithmetic)
/// - Excellent memory locality for allocated objects
/// - Simple bulk deallocation by resetting the arena
/// 
/// The allocator uses unsafe code and requires the 'Allow unsafe code' setting to be enabled.
/// All allocated memory must be of unmanaged types (no references to managed objects).
/// 
/// Usage pattern:
/// 1. Create arena with desired capacity
/// 2. Allocate objects using Alloc<T>()
/// 3. Use Reset() to clear all allocations and start over
/// 4. Call Dispose() when done to free the underlying memory
/// </summary>
public unsafe class ArenaAllocator : IDisposable {
    /// <summary>
    /// Pointer to the start of the allocated memory buffer.
    /// This is the base address from which all arena allocations are made.
    /// </summary>
    byte* buffer;
    
    /// <summary>
    /// Current offset into the buffer where the next allocation will occur.
    /// This effectively tracks how much of the arena has been used.
    /// </summary>
    public int offset;
    
    /// <summary>
    /// Total size of the arena in bytes. Once offset reaches this value,
    /// the arena is full and no more allocations can be made.
    /// </summary>
    readonly int capacity;

    /// <summary>
    /// Initializes a new arena allocator with the specified capacity.
    /// </summary>
    /// <param name="sizeInBytes">The total size of the arena in bytes. This memory will be allocated immediately.</param>
    /// <exception cref="System.OutOfMemoryException">Thrown if the system cannot allocate the requested memory.</exception>
    public ArenaAllocator(int sizeInBytes) {
        // Allocate unmanaged memory with 16-byte alignment for optimal performance
        buffer = (byte*)UnsafeUtility.Malloc(sizeInBytes, 16, Allocator.Persistent);
        offset = 0;
        capacity = sizeInBytes;
    }

    /// <summary>
    /// Allocates memory for one or more instances of the specified unmanaged type.
    /// The allocation is performed by advancing the offset pointer - no actual memory allocation occurs,
    /// just pointer arithmetic, making this extremely fast.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to allocate. Must not contain any managed references.</typeparam>
    /// <param name="count">Number of instances to allocate space for. Defaults to 1.</param>
    /// <returns>A pointer to the allocated memory, properly aligned for type T.</returns>
    /// <exception cref="Exception">Thrown if there isn't enough space remaining in the arena.</exception>
    public T* Alloc<T>(int count = 1) where T : unmanaged {
        // Calculate total bytes needed for the allocation
        int size = UnsafeUtility.SizeOf<T>() * count;
        
        // Check if we have enough space remaining in the arena
        if (offset + size > capacity) throw new Exception("Arena overflow");
        
        // Calculate the pointer to return (current position in buffer)
        T* ptr = (T*)(buffer + offset);
        
        // Advance the offset for the next allocation
        offset += size;
        
        return ptr;
    }
    
    /// <summary>
    /// Resets the arena by setting the offset back to 0, effectively "freeing" all allocations.
    /// This doesn't actually free any memory - it just makes the entire arena available for reuse.
    /// This is much faster than individual deallocations and is the primary benefit of arena allocation.
    /// 
    /// Note: Any pointers obtained from previous Alloc() calls become invalid after calling Reset().
    /// </summary>
    public void Reset() => offset = 0;

    /// <summary>
    /// Releases the unmanaged memory allocated by this arena.
    /// This should be called when the arena is no longer needed to prevent memory leaks.
    /// After calling Dispose(), this arena instance should not be used again.
    /// </summary>
    public void Dispose() {
        // Only free if we actually have allocated memory
        if (buffer != null) {
            UnsafeUtility.Free(buffer, Allocator.Persistent);
            buffer = null;
        }
        offset = 0;
    }
}