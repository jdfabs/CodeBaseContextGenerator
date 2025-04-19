public class HelloWorld {
    public void sayHello() {
        System.out.println("Hello");
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

class FriendlyGreeter implements Greeter, Welcomer {
    public void greet() {
        System.out.println("Greetings");
    }

    public void welcome() {
        System.out.println("Welcome");
    }
}