namespace Infrastructure;

public class IncrementCommand
{
    // Because of the required keyword here, a null key in a PUT body will return a 400 with message "The Key field is required". 
    // Note that this is a framework rejection; the controller endpoint will never actually be called.
    public required string Key { get; set; }    
    public long? PreviousValue { get; set; }
}
