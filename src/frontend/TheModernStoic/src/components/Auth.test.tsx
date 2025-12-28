import { useAuth0 } from "@auth0/auth0-react";
import { describe, expect, it, vi } from "vitest";
import { LoginButton, LogoutButton } from "./Auth";
import { render, screen } from "@testing-library/react";
import userEvent from '@testing-library/user-event';


// We mocks the module so we control what useAuth0 returns
vi.mock('@auth0/auth0-react');

describe("LoginButton Component", () =>{
    it("renders login button when the user is not authenticated", () =>{
        // Setup Mock Return Value
        (useAuth0 as any).mockReturnValue({
            isAuthenticated: false,
            loginWithRedirect: vi.fn(),
        });

        render(<LoginButton />);
                                                //regex that finds the button by the name "begin session"
        const button = screen.getByRole('button', { name: /begin session/i});
        expect(button).toBeInTheDocument();
    });

    it("calls loginWithRedirect when clicked", async () =>{
        const loginMock = vi.fn();

        (useAuth0 as any).mockReturnValue({
            isAuthenticated: false,
            loginWithRedirect: loginMock,
        });

        render(<LoginButton />);

        //use userEvent for realistic interaction
        const user = userEvent.setup();
        const button = screen.getByRole('button', { name: /begin session/i});

        await user.click(button);

        expect(loginMock).toHaveBeenCalledTimes(1);

    })
})

describe("LogoutButton Component", () =>{
    it("renders logout button when the user is authenticated", () =>{
        (useAuth0 as any).mockReturnValue({
            isAuthenticated: true,
            logout: vi.fn(),
        });

        render(<LogoutButton />);

        const button = screen.getByRole('button', { name: /end session/i});
        expect(button).toBeInTheDocument();
    });

    it("calls logout when clicked", async () =>{
        const logoutMock = vi.fn();

        (useAuth0 as any).mockReturnValue({
            isAuthenticated: true,
            logout: logoutMock,
        });

        render(<LogoutButton />);

                //use userEvent for realistic interaction
        const user = userEvent.setup();
        const button = screen.getByRole('button', { name: /end session/i});

        await user.click(button);

        expect(logoutMock).toHaveBeenCalledTimes(1);
    })
})