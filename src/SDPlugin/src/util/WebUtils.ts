export class WebUtils {
    public static async urlContentToDataUri(url: string): Promise<string> {
        return  fetch(url)
            .then( response => response.blob() )
            .then( blob => new Promise( callback =>{
                let reader = new FileReader() ;
                reader.onload = function(){ callback(this.result!.toString()!) } ;
                reader.readAsDataURL(blob) ;
            }) ) ;
    }

}