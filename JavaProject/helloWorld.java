public class HelloWorld {
    public void sayHello() {
        System.out.println("Hello");
        new Speaker(2);
        new Speaker2();
        
    }
}

interface Speaker {
    void speak();
    
}

interface Greeter {
    void greet();
    
}

private interface Welcomer {
    void welcome();
    
}

class FriendlyGreeter implements Greeter, Welcomer  {
    public void greet()  throws ArithmeticException{
        System.out.println("Greetings1");
        new Speaker();
        throw new ArithmeticException();
        
    }

    public void welcome() {
        System.out.println("Welcome1");
        
    }
}