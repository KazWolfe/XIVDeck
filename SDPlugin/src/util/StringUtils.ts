export class StringUtils {
    public static toTitleCase(myString: string) : string {
        return myString.replace(/\w\S*/g, function(txt){
            return txt.charAt(0).toUpperCase() + txt.slice(1);
        });
    }
}