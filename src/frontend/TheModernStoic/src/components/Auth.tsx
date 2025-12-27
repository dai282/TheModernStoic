import { useAuth0 } from "@auth0/auth0-react"

export const LoginButton = () =>{
    const {loginWithRedirect} = useAuth0();

    return (
        <button
            onClick={()=> loginWithRedirect()}
            className="px-6 py-2 border border-stoic-charcoal text-stoic-charcoal 
            hover:bg-stoic-charcoal hover:text-stoic-paper transition-colors font-serif italic">
            Begin Session
        </button>
    );
}

export const LogoutButton = () =>{
    const {logout} = useAuth0();

    return (
        <button
            onClick={()=> logout()}
            className="text-sm text-stoic-sand hover:text-stoic-accent underline">
            End Session
        </button>
    );
}