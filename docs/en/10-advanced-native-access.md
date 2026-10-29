# Advanced: low-level field access (`NativeApi`)

**Last resort.** A lot of the game's internal state is on private fields with no public property, event, or
method covering it - normal reflection (`GetField`/`GetValue`) doesn't work on them either (IL2CPP private
fields aren't reflectable the usual way). `C2ML.API.Native.NativeApi` reads them directly, bypassing IL2CPP's
type system entirely.

**Read this whole page before using it.** Unlike everything else in the API, `NativeApi` does **no validation**:
a wrong type for a field crashes the game (invalid memory access) instead of throwing a catchable exception.
Always check first whether a public API already covers what you need - a property, an event, a method on an
interface the game exposes - before reaching for this.

## Finding the right field

You need the field's exact name and the exact type that declares it. The Cpp2IL dump that ships with the
loader's own development tooling is the way to find both reliably; guessing a name is how you end up reading
the wrong field. If you only know roughly where to look, `DumpFieldNames` lists every real native field name on
a type:

```csharp
string[] fields = NativeApi.DumpFieldNames(someType);
foreach (string f in fields) Logger.Log(f);
```

## Reading a field

**Object/reference field** (a class or interface):

```csharp
SomeType value = NativeApi.GetObjectField<SomeType>(instance, typeof(DeclaringType), "m_someField");
```

**Value field** (`int`, `bool`, `float`, `double`, `IntPtr`, a blittable struct...):

```csharp
if (NativeApi.TryGetValueField<int>(instance, typeof(DeclaringType), "m_someField", out int value))
{
    // value is only valid if TryGetValueField returned true
}
```

- `instance` is the object instance the field lives on (an IL2CPP-wrapped object you already have, from a
  property, a method return value, an event...).
- `typeof(DeclaringType)` must be the type that actually **declares** the field - not a subclass, not an
  interface it implements. For a field declared on a base class, pass the base class's type, not the concrete
  instance's type.
- `GetObjectField<T>` returns `null` if the field doesn't exist, is `null` on the game's side, or if `T` doesn't
  actually match the real field type (silent failure, not an exception - see the warning above).
- `TryGetValueField<T>` returns `false` (and leaves `value` as `default`) in the same situations - always check
  the return value, never trust `value` on its own.

## Pointer of an instance

```csharp
IntPtr ptr = NativeApi.GetPointer(someIl2CppInstance);   // 0 if it isn't a valid IL2CPP object
```

Rarely needed directly - mostly useful while debugging (logging a pointer to compare two instances, etc.).

## Where to go next

| Guide | Covers |
|---|---|
| [09-sync.md](09-sync.md) | Messages, room roster and user variables between players |
