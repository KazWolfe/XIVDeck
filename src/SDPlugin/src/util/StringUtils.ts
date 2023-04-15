export class StringUtils {
    public static toTitleCase(myString: string) : string {
        return myString.replace(/\w\S*/g, function(txt){
            return txt.charAt(0).toUpperCase() + txt.slice(1);
        });
    }
    
    public static expandCaps(myString: string): string {
        return myString.split(/(?=[A-Z])/).join(" ");
    }
}